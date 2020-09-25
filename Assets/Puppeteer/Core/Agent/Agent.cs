using Puppeteer.Core.Action;
using Puppeteer.Core.External;
using Puppeteer.Core.Helper;
using Puppeteer.Core.Planning;
using Puppeteer.Core.Sensor;
using Puppeteer.Core.WorldState;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puppeteer.Core
{
	public class Agent<TKey, TValue> : MonoBehaviour, IAgent<TKey, TValue>
	{
		public void AddAction(IAction<TKey, TValue> _action)
		{
			m_ActionSet.Add(_action);
		}

		public void AddGoal(Goal<TKey, TValue> _goal)
		{
			m_GoalSet.Add(_goal);
		}

		public void AddOrUpdateWorkingMemory(TKey _targetKey, TValue _value)
		{
			m_WorkingMemory.AddOrUpdate(_targetKey, _value);
			OnWorkingMemoryChanged?.Invoke(m_WorkingMemory);
		}

		public void AddSensor(ISensor<TKey, TValue> _sensor)
		{
			_sensor.Initialise(this);
			m_Sensors.Add(_sensor);
		}

		public virtual FastPriorityQueue<SortableGoal<TKey, TValue>> CalculateSortedGoals()
		{
			if (m_SortedGoals == null)
			{
				m_SortedGoals = new FastPriorityQueue<SortableGoal<TKey, TValue>>(m_GoalSet.Count);
			}

			foreach (Goal<TKey, TValue> goal in m_GoalSet)
			{
				if (goal is SortableGoal<TKey, TValue> sortableGoal)
				{
					m_SortedGoals.Enqueue(sortableGoal, sortableGoal.Priority);
				}
			}

			return m_SortedGoals;
		}

		public void ClearActions()
		{
			m_ActionSet.Clear();
		}

		public void ClearGoals()
		{
			m_GoalSet.Clear();
		}

		public void ClearPlan()
		{
			m_Plan?.Clear();
			if (m_ActiveAction.GetActionState() == ActionState.Running)
			{
				m_ActiveAction?.Cancel();
			}

			m_ActiveAction = null;

			OnPlanInterrupted?.Invoke();

			m_ActiveGoal = null;
			m_IsPlanLocked = false;
		}

		public void ClearPlanHierarchy()
		{
			m_RootHierarchyNode = null;
		}

		public void ClearSensors()
		{
			m_Sensors.Clear();
		}

		public void ClearWorkingMemory()
		{
			m_WorkingMemory.Clear();
			OnWorkingMemoryChanged?.Invoke(m_WorkingMemory);
		}

		public void CreatePlanHierarchy(PlannerNode<TKey, TValue> _finalPathNode, IEnumerable<PlannerNode<TKey, TValue>> _openNodes, IEnumerable<PlannerNode<TKey, TValue>> _closedNodes)
		{
			List<PlannerHierarchyNodePair<TKey, TValue>> nodePairs = new List<PlannerHierarchyNodePair<TKey, TValue>>();

			PuppeteerRuntimeHelper.CreateNodePairsForClosedNodes(_closedNodes, ref nodePairs);

			var goalNodePair = nodePairs.Find(_entry => _entry.PlannerNodeInstance.GetParent() == null);

			m_RootHierarchyNode = goalNodePair?.HierarchyNodeInstance;

			PuppeteerRuntimeHelper.CreateNodePairsForOpenNodes(_openNodes, ref nodePairs);

			PuppeteerRuntimeHelper.CreateNodePairForLastOpenNode(ref _finalPathNode, ref nodePairs);
			var lastHierarchyNode = nodePairs.Last().HierarchyNodeInstance;

			PuppeteerRuntimeHelper.ConstructHierarchy(ref nodePairs);

			PuppeteerRuntimeHelper.SetPartOfFoundPathRecursive(lastHierarchyNode);
		}

		public void ExecutePlan()
		{
			if (m_ActiveAction == null)
			{
				return;
			}

			if (m_ActiveAction.GetActionState() == ActionState.Inactive)
			{
				m_ActiveAction.Initalise(OnActionCompleted, OnActionFailed);
				m_ActiveAction.Enter(this);
			}

			if (m_ActiveAction.GetActionState() == ActionState.Running)
			{
				m_ActiveAction.Execute();
			}

			switch (m_ActiveAction.GetActionState())
			{
				case ActionState.Running:
					return;

				case ActionState.Completed:
					m_ActiveAction.Exit();
					++m_IndexInPlan;
					OnWorkingMemoryChanged?.Invoke(GetWorkingMemory());
					break;

				case ActionState.Failed:
					m_ActiveAction.Cancel();
					break;
			}
		}

		public void FixedUpdate()
		{
			Tick();
		}

		public ActionSet<TKey, TValue> GetActionSet()
		{
			return m_ActionSet;
		}

		public IAction<TKey, TValue> GetActiveAction()
		{
			return m_ActiveAction;
		}

		public GoalSet<TKey, TValue> GetGoalSet()
		{
			return m_GoalSet;
		}

		public int GetIndexInPlan()
		{
			return m_IndexInPlan;
		}

		public HierarchyNode<TKey, TValue> GetRootHierarchyNode()
		{
			return m_RootHierarchyNode;
		}

		public WorldState<TKey, TValue> GetWorkingMemory()
		{
			return m_WorkingMemory;
		}

		public bool HasPlan()
		{
			return m_ActiveAction != null || (m_Plan != null && !m_Plan.Empty());
		}

		public virtual bool IsGoalBetterThanActiveGoal(Goal<TKey, TValue> _goal)
		{
			return true;
		}

		public void LockPlan(bool _locked = true)
		{
			m_IsPlanLocked = _locked;
		}

		public void SetActiveGoal(Goal<TKey, TValue> _goal)
		{
			m_ActiveGoal = _goal;
		}

		public void SetUnhandledSensorChanges(bool _value)
		{
			m_UnhandledSensorChanges = _value;
		}

		public void SetWorkingMemory(WorldState<TKey, TValue> _worldState)
		{
			m_WorkingMemory = _worldState;
		}

		public bool ShouldReplan()
		{
			return (!m_IsPlanLocked && !HasPlan() && (Time.realtimeSinceStartup - m_LastPlanningTimeInSeconds > m_ReplanLimiterInSeconds))
					|| (!m_IsPlanLocked && m_UnhandledSensorChanges);
		}

		public void SortSensors()
		{
			m_Sensors.SortByOrder();
		}

		public void Tick()
		{
			UpdateSensors();
			UpdateGoals();
			UpdatePlan();
			ExecutePlan();
		}

		public void UpdateSensors()
		{
			for (int i = 0; i < m_Sensors.Count; ++i)
			{
				ISensor<TKey, TValue> sensor = m_Sensors[i];
				if (sensor.GetShouldBeTicked() || Time.frameCount % (sensor.GetTickRate() + i) != 0)
				{
					continue;
				}

				m_UnhandledSensorChanges |= sensor.DetectWorldStateChange(this);
			}

			if (m_UnhandledSensorChanges)
			{
				OnWorkingMemoryChanged?.Invoke(m_WorkingMemory);
			}
		}

		protected void PlanGenerated(GoalPlanPair<TKey, TValue> _goalPlanPair)
		{
			m_UnhandledSensorChanges = false;
			m_LastPlanningTimeInSeconds = Time.realtimeSinceStartup;

			if (_goalPlanPair.GoalInstance == null)
			{
				return;
			}

			OnBeforeNewPlan?.Invoke();
			m_ActiveAction?.Cancel();
			SetActiveGoal(_goalPlanPair.GoalInstance);

			m_Plan = _goalPlanPair.PlanInstance;
			m_IsPlanLocked = false;
			m_IndexInPlan = 0;

			if (!m_Plan.Empty())
			{
				m_ActiveAction = m_Plan.Pop();
			}
			else
			{
				m_ActiveAction = null;
			}

			OnNewPlan?.Invoke();
			OnPlanProgressed?.Invoke();
		}

		protected virtual void UpdatePlan()
		{
		}

		private void OnActionCompleted()
		{
			if (m_Plan.Empty())
			{
				m_ActiveAction = null;
				m_ActiveGoal = null;
				m_IsPlanLocked = false;
			}
			else
			{
				m_ActiveAction = m_Plan.Pop();
			}

			OnPlanProgressed?.Invoke();
		}

		private void OnActionFailed()
		{
			// Action failed. Clear plan to force replanning
			ClearPlan();
		}

		private void UpdateGoals()
		{
			// #COULD: Sensor reaction time check before replanning
		}

		public System.Action OnPlanInterrupted;
		public System.Action OnBeforeNewPlan;
		public System.Action OnNewPlan;
		public System.Action OnPlanProgressed;
		public System.Action<WorldState<TKey, TValue>> OnWorkingMemoryChanged;

		protected readonly ActionSet<TKey, TValue> m_ActionSet = new ActionSet<TKey, TValue>();
		protected readonly GoalSet<TKey, TValue> m_GoalSet = new GoalSet<TKey, TValue>();
		protected readonly SensorList<TKey, TValue> m_Sensors = new SensorList<TKey, TValue>();

		protected Goal<TKey, TValue> m_ActiveGoal = null;
		protected Plan<TKey, TValue> m_Plan = null;
		protected FastPriorityQueue<SortableGoal<TKey, TValue>> m_SortedGoals = null;
		protected WorldState<TKey, TValue> m_WorkingMemory = new WorldState<TKey, TValue>();

		private readonly float m_ReplanLimiterInSeconds = 1f;

		private IAction<TKey, TValue> m_ActiveAction = null;
		private int m_IndexInPlan = 0;
		private bool m_IsPlanLocked = false;
		private float m_LastPlanningTimeInSeconds = 0;
		private HierarchyNode<TKey, TValue> m_RootHierarchyNode = null;
		private bool m_UnhandledSensorChanges = false;
	}
}