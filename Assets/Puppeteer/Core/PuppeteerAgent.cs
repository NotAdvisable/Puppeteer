using Puppeteer.Core.Configuration;
using Puppeteer.Core.Helper;
using Puppeteer.Core.WorldState;
using System;
using UnityEngine;

namespace Puppeteer.Core
{
	public class PuppeteerAgent : Agent<string, object>
	{
		public Guid ArchetypeGUID
		{
			get => m_ArchetypeGUID.Value;
			set
			{
				Guid oldGuid = m_ArchetypeGUID.Value;
				m_ArchetypeGUID = value;
				OnArchetypeChanged?.Invoke(oldGuid, value);
			}
		}

		public void AddAction(ActionDescription _actionDescription)
		{
			WorldStateModifier<string, object> preconditions = new WorldStateModifier<string, object>();
			foreach (WorldStateDescription descriptionPrecondition in _actionDescription.Preconditions)
			{
				preconditions.Add(descriptionPrecondition.Key, descriptionPrecondition.Value);
			}

			WorldStateModifier<string, object> effects = new WorldStateModifier<string, object>();
			foreach (WorldStateDescription descriptionEffect in _actionDescription.Effects)
			{
				effects.Add(descriptionEffect.Key, descriptionEffect.Value);
			}

			PuppeteerExecutableAction action = Activator.CreateInstance(_actionDescription.ExecutableActionType) as PuppeteerExecutableAction;

			// #performancePotential: We could use the ExecutableAction constructor in Activator.CreateInstance to set these values,
			// though that would mean every subclass of PuppeteerExecutableAction would have to implement that constructor as well.
			// To keep the implementation of these user created classes as clean as possible, we rather set the values after creating the instance.
			// Another solution would be to generate the constructor for the user which could benefit performance.

			action.SetLabel(_actionDescription.DisplayName);
			action.SetBaseUtility(_actionDescription.BaseUtility);
			action.SetPreconditions(preconditions);
			action.SetEffects(effects);

			action.DescriptionGUID = _actionDescription.GUID;

			AddAction(action);
		}

		public void AddGoal(GoalDescription _goalDescription)
		{
			PuppeteerGoal goal = new PuppeteerGoal
			{
				DescriptionGUID = _goalDescription.GUID,
			};

			foreach (WorldStateDescription goalPart in _goalDescription.GoalParts)
			{
				goal.Add(goalPart.Key, goalPart.Value);
			}

			AddGoal(goal);
		}

		public void AddSensor(SensorDescription _sensorDescription)
		{
			PuppeteerExecutableSensor sensor = Activator.CreateInstance(_sensorDescription.ExecutableSensorType) as PuppeteerExecutableSensor;

			// #performancePotential same as above. We could use the ExecutableSensor constructor in Activator.CreateInstance.

			sensor.SetOrder(_sensorDescription.ExecutionOrder);
			sensor.SetTickRate(_sensorDescription.TickRate);
			sensor.SetManagedWorldState(_sensorDescription.ManagedWorldState);

			sensor.DescriptionGUID = _sensorDescription.GUID;

			AddSensor(sensor);
		}

		public void Awake()
		{
			PuppeteerManager.Instance.InitialiseAgentUsingArchetype(this, ArchetypeGUID);

			OnArchetypeChanged += (_oldGuid, _newGuid) =>
			{
				ClearPlan();
				ClearWorkingMemory();
				PuppeteerManager.Instance.InitialiseAgentUsingArchetype(this, _newGuid);
			};
		}

		public override External.FastPriorityQueue<SortableGoal<string, object>> CalculateSortedGoals()
		{
			if (m_SortedGoals == null)
			{
				m_SortedGoals = new External.FastPriorityQueue<SortableGoal<string, object>>(m_GoalSet.Count);
			}

			m_SortedGoals.Clear();

			foreach (Goal<string, object> goal in m_GoalSet)
			{
				if (goal is PuppeteerGoal puppeteerGoal)
				{
					GoalDescription goalDesc = PuppeteerManager.Instance.GetGoalDescription(puppeteerGoal.DescriptionGUID);
					float goalUtility = goalDesc.BaseUtility;

					for (int i = 0; i < goalDesc.UtilityParts.Length; ++i)
					{
						EvaluateAndAddUtility(ref goalUtility, ref goalDesc.UtilityParts[i]);
					}

					float priority = goalUtility > 0 ? 1 / goalUtility : 1;
					m_SortedGoals.Enqueue(puppeteerGoal, priority);
				}
			}

			return m_SortedGoals;
		}

		public PuppeteerGoal GetActiveGoal()
		{
			return m_ActiveGoal as PuppeteerGoal;
		}

		public Plan<string, object> GetPlan()
		{
			return m_Plan;
		}

		public override bool IsGoalBetterThanActiveGoal(Goal<string, object> _goal)
		{
			if (m_ActiveGoal == null)
			{
				return base.IsGoalBetterThanActiveGoal(_goal);
			}

			if (m_ActiveGoal is PuppeteerGoal activePuppeteerGoal && _goal is PuppeteerGoal newPuppeteerGoal)
			{
				GoalDescription activeGoalDesc = PuppeteerManager.Instance.GetGoalDescription(activePuppeteerGoal.DescriptionGUID);
				float activeGoalUtility = activeGoalDesc.BaseUtility;
				for (int i = 0; i < activeGoalDesc.UtilityParts.Length; ++i)
				{
					EvaluateAndAddUtility(ref activeGoalUtility, ref activeGoalDesc.UtilityParts[i]);
				}

				GoalDescription newGoalDesc = PuppeteerManager.Instance.GetGoalDescription(newPuppeteerGoal.DescriptionGUID);
				float newGoalUtility = newGoalDesc.BaseUtility;
				for (int i = 0; i < newGoalDesc.UtilityParts.Length; ++i)
				{
					EvaluateAndAddUtility(ref newGoalUtility, ref newGoalDesc.UtilityParts[i]);
				}

				if (newGoalUtility > activeGoalUtility)
				{
					return base.IsGoalBetterThanActiveGoal(_goal);
				}
			}

			return false;
		}

		public void OnDestroy()
		{
			OnPuppeteerAgentDestroy?.Invoke(this);
		}

		public void OnEnable()
		{
			PuppeteerManager.Instance.RegisterAgent(this);
		}

		protected override void UpdatePlan()
		{
			if (ShouldReplan())
			{
				PlanGenerated(PuppeteerManager.Instance.GeneratePlanForAgent(this));
			}
		}

		private void EvaluateAndAddUtility(ref float _targetUtility, ref UtilityDescription _utilityDescription)
		{
			if (!m_WorkingMemory.ContainsKey(_utilityDescription.WorldStateName))
			{
				return;
			}

			float sampleValueAsFloat = m_WorkingMemory[_utilityDescription.WorldStateName].StructConvertTo<float>();

			switch (_utilityDescription.UtilityOperator)
			{
				case Utility.UtilityOperators.PLUS_EQUALS:
					_targetUtility += _utilityDescription.UtilityCurve.Evaluate(sampleValueAsFloat) * _utilityDescription.CurveMultiplier;
					break;

				case Utility.UtilityOperators.MINUS_EQUALS:
					_targetUtility -= _utilityDescription.UtilityCurve.Evaluate(sampleValueAsFloat) * _utilityDescription.CurveMultiplier;
					break;

				case Utility.UtilityOperators.MULTIPLY_EQUALS:
					_targetUtility *= _utilityDescription.UtilityCurve.Evaluate(sampleValueAsFloat) * _utilityDescription.CurveMultiplier;
					break;

				case Utility.UtilityOperators.DIVIDE_EQUALS:
					_targetUtility /= _utilityDescription.UtilityCurve.Evaluate(sampleValueAsFloat) * _utilityDescription.CurveMultiplier;
					break;

				case Utility.UtilityOperators.MODULO_EQUALS:
					_targetUtility %= _utilityDescription.UtilityCurve.Evaluate(sampleValueAsFloat) * _utilityDescription.CurveMultiplier;
					break;

				default:
					break;
			}
		}

		public Action<Guid, Guid> OnArchetypeChanged;
		public Action<PuppeteerAgent> OnPuppeteerAgentDestroy;

		[SerializeField]
		private SerialisableGuid m_ArchetypeGUID = Guid.Empty;
	}
}