using Puppeteer.Core.Configuration;
using Puppeteer.Core.Debug;
using Puppeteer.Core.Helper;
using Puppeteer.Core.Planning;
using System;
using System.Collections.Generic;

namespace Puppeteer.Core
{
	public class PuppeteerManager : SingletonManager<PuppeteerManager>
	{
		public PuppeteerManager()
		{
			m_Planner = new Planner<string, object>();

			Type goalDescType = typeof(GoalDescription);
			Type actionDescType = typeof(ActionDescription);
			Type sensorDescType = typeof(SensorDescription);
			Type archetypeDescType = typeof(ArchetypeDescription);

			m_GetDescriptionDict = new Dictionary<Type, Func<Guid, object>>
			{
				{ goalDescType, GetGoalDescription },
				{ actionDescType, GetActionDescription },
				{ sensorDescType, GetSensorDescription },
				{ archetypeDescType, GetArchetypeDescription },
			};

			m_GetDescriptionsDict = new Dictionary<Type, Func<object>>
			{
				{ goalDescType, GetGoalDescriptions },
				{ actionDescType, GetActionDescriptions },
				{ sensorDescType, GetSensorDescriptions },
				{ archetypeDescType, GetArchetypeDescriptions },
			};

			m_ReloadTypeDict = new Dictionary<Type, System.Action>
			{
				{ goalDescType, ReloadGoals },
				{ actionDescType, ReloadActions },
				{ sensorDescType, ReloadSensors },
				{ archetypeDescType, ReloadArchetypes },
			};
		}

		public GoalPlanPair<string, object> GeneratePlanForAgent(PuppeteerAgent _agent)
		{
			return m_Planner.GenerateGoalPlanPairForAgent(_agent);
		}

		public ActionDescription GetActionDescription(Guid _guid)
		{
			return GetActionDescriptions()?.Find(_entry => _entry.GUID == _guid);
		}

		public List<ActionDescription> GetActionDescriptions()
		{
			if (m_SerialisedActions == null)
			{
				ReloadActions();
			}

			return m_SerialisedActions;
		}

		public HashSet<PuppeteerAgent> GetAgents()
		{
#if UNITY_EDITOR
			m_Agents.Clear();
			// Agents only register themselves in play mode.
			// Since I don't want them to run in edit mode, we have to find them.
			PuppeteerRuntimeHelper.AddAllToHashSet(ref m_Agents, UnityEngine.Object.FindObjectsOfType<PuppeteerAgent>());
#endif
			return m_Agents;
		}

		public ArchetypeDescription GetArchetypeDescription(Guid _guid)
		{
			return GetArchetypeDescriptions()?.Find(_entry => _entry.GUID == _guid);
		}

		public List<ArchetypeDescription> GetArchetypeDescriptions()
		{
			if (m_SerialisedArchetypes == null)
			{
				ReloadArchetypes();
			}

			return m_SerialisedArchetypes;
		}

		public T GetDescriptionOfType<T>(Guid _guid) where T : BasicDescription
		{
			return m_GetDescriptionDict[typeof(T)]?.Invoke(_guid) as T;
		}

		public List<T> GetDescriptionsOfType<T>() where T : BasicDescription
		{
			return m_GetDescriptionsDict[typeof(T)]?.Invoke() as List<T>;
		}

		public GoalDescription GetGoalDescription(Guid _guid)
		{
			return GetGoalDescriptions()?.Find(_entry => _entry.GUID == _guid);
		}

		public List<GoalDescription> GetGoalDescriptions()
		{
			if (m_SerialisedGoals == null)
			{
				ReloadGoals();
			}

			return m_SerialisedGoals;
		}

		public string GetGoalName(Guid _guid)
		{
			var goals = GetGoalDescriptions();
			var entryForGUID = m_SerialisedGoals.Find(_entry => _entry.GUID == _guid);

			return entryForGUID != null ? entryForGUID.DisplayName : "Invalid GUID";
		}

		public SensorDescription GetSensorDescription(Guid _guid)
		{
			return GetSensorDescriptions()?.Find(_entry => _entry.GUID == _guid);
		}

		public List<SensorDescription> GetSensorDescriptions()
		{
			if (m_SerialisedSensors == null)
			{
				ReloadSensors();
			}

			return m_SerialisedSensors;
		}

		public bool InitialiseAgentUsingArchetype(PuppeteerAgent _agent, Guid _archetypeGuid)
		{
			if(_archetypeGuid == Guid.Empty)
			{
				PuppeteerLogger.Log(string.Format("Agent {0} has no Archetype configured.", _agent), LogType.Warning);
			}

			m_DescriptionCache.ArchetypeDesc = GetArchetypeDescription(_archetypeGuid);
			if (m_DescriptionCache.ArchetypeDesc == null)
			{
				PuppeteerLogger.Log(string.Format("Archetype with GUID [{0}] couldn't be loaded", _archetypeGuid.ToString()), LogType.Error);
				return false;
			}

			_agent.ClearGoals();

			for (int i = 0; i < m_DescriptionCache.ArchetypeDesc.GoalGUIDs.Length; ++i)
			{
				m_DescriptionCache.GoalDesc = GetGoalDescription(m_DescriptionCache.ArchetypeDesc.GoalGUIDs[i]);

				if (m_DescriptionCache.GoalDesc != null)
				{
					_agent.AddGoal(m_DescriptionCache.GoalDesc);
				}
			}

			_agent.ClearActions();

			for (int i = 0; i < m_DescriptionCache.ArchetypeDesc.ActionGUIDs.Length; ++i)
			{
				m_DescriptionCache.ActionDesc = GetActionDescription(m_DescriptionCache.ArchetypeDesc.ActionGUIDs[i]);

				if (m_DescriptionCache.ActionDesc != null)
				{
					_agent.AddAction(m_DescriptionCache.ActionDesc);
				}
			}

			_agent.ClearSensors();

			for (int i = 0; i < m_DescriptionCache.ArchetypeDesc.SensorGUIDs.Length; ++i)
			{
				m_DescriptionCache.SensorDesc = GetSensorDescription(m_DescriptionCache.ArchetypeDesc.SensorGUIDs[i]);

				if (m_DescriptionCache.SensorDesc != null)
				{
					_agent.AddSensor(m_DescriptionCache.SensorDesc);
				}
			}

			_agent.SortSensors();

			return true;
		}

		public void LoadDescriptionsOfType<T>() where T : BasicDescription
		{
			m_ReloadTypeDict[typeof(T)]?.Invoke();
		}

		public void RegisterAgent(PuppeteerAgent _agent)
		{
			m_Agents.Add(_agent);
		}

		public bool SaveActions()
		{
			return Serialiser.SaveActions(m_SerialisedActions);
		}

		public bool SaveArchetypes()
		{
			return Serialiser.SaveArchetypes(m_SerialisedArchetypes);
		}

		public bool SaveGoals()
		{
			return Serialiser.SaveGoals(m_SerialisedGoals);
		}

		public bool SaveSensors()
		{
			return Serialiser.SaveSensors(m_SerialisedSensors);
		}

		private void ReloadActions()
		{
			m_SerialisedActions = Serialiser.LoadActions();
		}

		private void ReloadArchetypes()
		{
			m_SerialisedArchetypes = Serialiser.LoadArchetypes();
		}

		private void ReloadGoals()
		{
			m_SerialisedGoals = Serialiser.LoadGoals();
		}

		private void ReloadSensors()
		{
			m_SerialisedSensors = Serialiser.LoadSensors();
		}

		private readonly DescriptionCache m_DescriptionCache = new DescriptionCache();

		private readonly Dictionary<Type, Func<Guid, object>> m_GetDescriptionDict;
		private readonly Dictionary<Type, Func<object>> m_GetDescriptionsDict;
		private readonly Planner<string, object> m_Planner;
		private readonly Dictionary<Type, System.Action> m_ReloadTypeDict;

		private HashSet<PuppeteerAgent> m_Agents = new HashSet<PuppeteerAgent>();

		private List<ActionDescription> m_SerialisedActions = null;
		private List<ArchetypeDescription> m_SerialisedArchetypes = null;
		private List<GoalDescription> m_SerialisedGoals = null;
		private List<SensorDescription> m_SerialisedSensors = null;

		private class DescriptionCache
		{
			public DescriptionCache()
			{
			}

			public ActionDescription ActionDesc;
			public ArchetypeDescription ArchetypeDesc;
			public GoalDescription GoalDesc;
			public SensorDescription SensorDesc;
		}
	}
}