using Puppeteer.Core;
using Puppeteer.Core.WorldState;
using Puppeteer.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PuppeteerAgent))]
public class PuppeteerAgentInspector : Editor
{
	public override VisualElement CreateInspectorGUI()
	{
		var visualTreeAsset = Resources.Load<VisualTreeAsset>("LayoutAgentInspector");
		m_InspectorContainer = new VisualElement();
		visualTreeAsset.CloneTree(m_InspectorContainer);

		LazyInitConfigurator();
		UpdateConfigurator();

		StyleSheet themeSheet = Resources.Load<StyleSheet>(EditorGUIUtility.isProSkin ? "StyleDarkTheme" : "StyleLightTheme");
		m_InspectorContainer.styleSheets.Add(themeSheet);

		m_InspectorContainer.styleSheets.Add(Resources.Load<StyleSheet>("StyleAgentInspector"));

		var agentIcon = m_InspectorContainer.Q<Image>(className: "agentIcon");
		agentIcon.image = PuppeteerEditorResourceLoader.AgentIcon32.texture;

		return m_InspectorContainer;
	}

	private void AgentArchetypeChanged(Guid _oldGuid, Guid _newGuid)
	{
		EnsureArchetypeSelectorIsInSync();
		m_AgentInspectorConfigurator.ArchetypeField.SetValueWithoutNotify(_newGuid);
	}

	private void AgentWorkingMemoryChanged(WorldState<string, object> _worldState)
	{
		foreach (KeyValuePair<string, object> memoryEntry in _worldState)
		{
			WorkingMemoryItem matchingItem = m_WorkingMemoryListItems.Find(_entry => _entry.GetKey().Equals(memoryEntry.Key));

			if (matchingItem == null)
			{
				WorkingMemoryItem newWorkingMemoryItem = new WorkingMemoryItem(memoryEntry);
				m_WorkingMemoryListItems.Add(newWorkingMemoryItem);
			}
			else
			{
				if (!memoryEntry.Value.Equals(matchingItem.GetValue()))
				{
					matchingItem.SetValueWithoutNotify(memoryEntry.Value);
				}
			}
		}

		UpdatePlanDisplay();
		UpdateWorkingMemoryDisplay();
	}

	private void ArchetypeFieldChangedEvent(ChangeEvent<System.Guid> _event)
	{
		m_TargetAgent.ArchetypeGUID = _event.newValue;
		EditorUtility.SetDirty(target);
	}

	private void EnsureArchetypeSelectorIsInSync()
	{
		ref var archetypeField = ref m_AgentInspectorConfigurator.ArchetypeField;

		if (!archetypeField.IsInSync())
		{
			PuppeteerEditorHelper.ReplaceArchetypeSelectorField(ref archetypeField, m_TargetAgent.ArchetypeGUID, ArchetypeFieldChangedEvent);
		}
	}

	private void LazyInitConfigurator()
	{
		if (m_AgentInspectorConfigurator != null)
		{
			return;
		}

		m_AgentInspectorConfigurator = new AgentInspectorConfigurator
		{
			ArchetypeField = m_InspectorContainer.Q<ArchetypeSelectorField>(name: "archetypeSelector"),
			WorkingMemoryContainer = m_InspectorContainer.Q<VisualElement>(name: "workingMemoryContainer"),
			PlanContainer = m_InspectorContainer.Q<VisualElement>(name: "planContainer"),
			OpenConfiguratorButton = m_InspectorContainer.Q<Button>(name: "openConfigurator"),
			OpenVisualiserButton = m_InspectorContainer.Q<Button>(name: "openVisualiser"),
		};

		RegisterConfiguratorCallbacks();
	}

	private void OnDisable()
	{
		m_TargetAgent.OnWorkingMemoryChanged -= AgentWorkingMemoryChanged;
		m_TargetAgent.OnArchetypeChanged -= AgentArchetypeChanged;
	}

	private void OnEnable()
	{
		m_TargetAgent = target as PuppeteerAgent;

		m_TargetAgent.OnWorkingMemoryChanged += AgentWorkingMemoryChanged;
		m_TargetAgent.OnArchetypeChanged += AgentArchetypeChanged;
	}

	private void RegisterConfiguratorCallbacks()
	{
		m_AgentInspectorConfigurator.ArchetypeField.RegisterValueChangedCallback(ArchetypeFieldChangedEvent);

		m_AgentInspectorConfigurator.ArchetypeField.RegisterCallback<MouseEnterEvent>(_event =>
		{
			EnsureArchetypeSelectorIsInSync();
		});

		m_AgentInspectorConfigurator.OpenConfiguratorButton.clickable.clicked += () =>
		{
			var window = PuppeteerConfiguratorWindow.CreateWindow();
			window.OpenEntryInTabOfType<AgentView>(m_TargetAgent.gameObject.GetInstanceID());
		};

		m_AgentInspectorConfigurator.OpenVisualiserButton.clickable.clicked += () =>
		{
			PuppeteerPlanVisualiserWindow.CreateWindow();
		};
	}

	private void UpdateConfigurator()
	{
		EnsureArchetypeSelectorIsInSync();
		m_AgentInspectorConfigurator.ArchetypeField.SetValueWithoutNotify(m_TargetAgent.ArchetypeGUID);
		AgentWorkingMemoryChanged(m_TargetAgent.GetWorkingMemory());
	}

	private void UpdatePlanDisplay()
	{
		m_AgentInspectorConfigurator.PlanContainer.Clear();

		if (m_TargetAgent.GetActiveAction() != null)
		{
			m_AgentInspectorConfigurator.PlanContainer.Add(new PlanItem(PlanItem.PlanItemType.ActiveAction, m_TargetAgent.GetActiveAction().GetLabel()));
		}

		Plan<string, object> plan = m_TargetAgent.GetPlan();
		if (plan != null)
		{
			foreach (var item in m_TargetAgent.GetPlan())
			{
				m_AgentInspectorConfigurator.PlanContainer.Add(new PlanItem(PlanItem.PlanItemType.InactiveAction, item.GetLabel()));
			}

			PuppeteerGoal activeGoal = m_TargetAgent.GetActiveGoal();
			if (activeGoal != null)
			{
				var goalDesc = PuppeteerManager.Instance.GetGoalDescription(activeGoal.DescriptionGUID);
				m_AgentInspectorConfigurator.PlanContainer.Add(new PlanItem(PlanItem.PlanItemType.ActiveGoal, goalDesc.DisplayName));
			}
		}
	}

	private void UpdateWorkingMemoryDisplay()
	{
		m_AgentInspectorConfigurator.WorkingMemoryContainer.Clear();
		m_WorkingMemoryListItems.Clear();

		foreach (KeyValuePair<string, object> memoryEntry in m_TargetAgent.GetWorkingMemory())
		{
			// #performancePotential: Cache working memory items instead of clearing and creating new ones

			WorkingMemoryItem workingMemoryItem = new WorkingMemoryItem(memoryEntry);

			m_AgentInspectorConfigurator.WorkingMemoryContainer.Add(workingMemoryItem);
			m_WorkingMemoryListItems.Add(workingMemoryItem);
		}
	}

	private readonly List<WorkingMemoryItem> m_WorkingMemoryListItems = new List<WorkingMemoryItem>();

	private AgentInspectorConfigurator m_AgentInspectorConfigurator = null;
	private VisualElement m_InspectorContainer;
	private PuppeteerAgent m_TargetAgent = null;

	private class AgentInspectorConfigurator
	{
		public ArchetypeSelectorField ArchetypeField;
		public Button OpenConfiguratorButton;
		public Button OpenVisualiserButton;
		public VisualElement PlanContainer;
		public VisualElement WorkingMemoryContainer;
	}
}