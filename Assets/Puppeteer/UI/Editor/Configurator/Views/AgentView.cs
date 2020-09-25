using Puppeteer.Core;
using Puppeteer.Core.WorldState;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class AgentView : PuppeteerView
	{
		public AgentView(VisualElement _rootElement, VisualElement _leftPanel, VisualElement _rightPanel, Action<int> _updateLastSelectedCallback)
			: base(_rootElement, _leftPanel, _rightPanel)
		{
			m_Label = "Agents";
			tooltip = "Configure and observe the agents that are currently in the scene.";

			var visualTreeAsset = Resources.Load<VisualTreeAsset>("LayoutAgentConfigurator");
			visualTreeAsset.CloneTree(m_ConfiguratorElement);

			m_OnUpdateSerialisedLastSelected = _updateLastSelectedCallback;
			OnListItemSelectedOrRemoved = ListItemSelectedOrRemoved;

			ReloadAgentListItems();

			EditorApplication.playModeStateChanged += EditorPlayModeStateChanged;
		}

		~AgentView()
		{
			EditorApplication.playModeStateChanged -= EditorPlayModeStateChanged;
		}

		public void AddPrefabIcon(VisualElement _iconContainer, PuppeteerAgent _agent)
		{
			_iconContainer.hierarchy.Clear();

			GameObject prefabObject = PrefabUtility.GetCorrespondingObjectFromSource(_agent.gameObject);
			string path = prefabObject != null ? AssetDatabase.GetAssetPath(prefabObject) : null;

			IMGUIContainer icon = new IMGUIContainer(() =>
			{
				if (path != null)
				{
					GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

					Editor editor = GetPreviewEditor(asset);

					editor.OnPreviewGUI(GUILayoutUtility.GetRect(170, 170), new GUIStyle() { normal = { background = Texture2D.whiteTexture } });
				}
				else
				{
					GUILayout.Label("Not a Prefab.");
				}
			})
			{
				focusable = false,
				name = "prefabIcon",
			};

			icon.AddToClassList("prefabIcon");

			_iconContainer.hierarchy.Add(icon);
		}

		public override void RegisterTabViewCallbacks(TabView _tabView)
		{
			_tabView.OnTabListItemDeleted += OtherTabArchetypeListItemDeleted;
		}


		private void OtherTabArchetypeListItemDeleted(ListItem _deletedListItem)
		{
			if (_deletedListItem is ArchetypeListItem deletedArchetypeListItem)
			{
				for (int i = 0; i < m_ListItems.Count; ++i)
				{
					if (m_ListItems[i] is AgentListItem agentListItem)
					{
						PuppeteerAgent agent = agentListItem.GetAgent();
						if (agent == null)
						{
							continue;
						}

						if (agent.ArchetypeGUID != deletedArchetypeListItem.GetDescription().GUID)
						{
							continue;
						}

						agent.ArchetypeGUID = Guid.Empty;
					}
				}
			}
		}

		public override void CleanUp()
		{
			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				if (m_ListItems[i] is AgentListItem agentListItem)
				{
					if (agentListItem.GetAgent() == null)
					{
						continue;
					}

					agentListItem.GetAgent().OnPuppeteerAgentDestroy -= PuppeteerAgentDestroy;
				}
			}
		}

		public override void CloseView()
		{
			base.CloseView();

			if (m_PreviewEditors != null)
			{
				// The editors need to be destroyed so the playableGraphs they contain don't leak memory.
				for (int i = 0; i < m_PreviewEditors.Count; ++i)
				{
					UnityEngine.Object.DestroyImmediate(m_PreviewEditors[0]);
				}

				m_PreviewEditors.Clear();
			}

			m_AddButton.visible = true;

			UnregisterWindowCallbacks();
			OnListItemDeselected -= ListItemDeselected;
		}

		public Editor GetPreviewEditor(GameObject _asset)
		{
			if (m_PreviewEditors == null)
			{
				m_PreviewEditors = new List<Editor>();
			}

			foreach (Editor editor in m_PreviewEditors)
			{
				if ((GameObject)editor.target == _asset)
				{
					return editor;
				}
			}
			Editor newEditor = Editor.CreateEditor(_asset);
			m_PreviewEditors.Add(newEditor);

			return newEditor;
		}

		public override void OpenView(SerialisedConfiguratorState _serialisedConfiguratorStates)
		{
			OpenView(_serialisedConfiguratorStates.LastOpenedAgentObjectID);
		}

		public void TryOpenEntry(int _instanceID)
		{
			var matchingItem = m_ListItems.Find(_entry => (_entry as AgentListItem).GetAgent().gameObject.GetInstanceID() == _instanceID);

			if (matchingItem != null)
			{
				UpdateSelectedListItem(matchingItem);
			}
		}

		protected override void LazyInitConfigurator()
		{
			if (m_AgentConfigurator != null)
			{
				return;
			}

			m_AgentConfigurator = new AgentConfigurator
			{
				ArchetypeField = m_ConfiguratorElement.Q<ArchetypeSelectorField>(name: "archetypeSelector"),
				AgentName = m_ConfiguratorElement.Q<TextField>(name: "gameObjectName"),
				PrefabIcon = m_ConfiguratorElement.Q<VisualElement>(name: "prefabIconFrame"),
				WorkingMemoryContainer = m_ConfiguratorElement.Q<VisualElement>(name: "workingMemoryContainer"),
				PlanContainer = m_ConfiguratorElement.Q<VisualElement>(name: "planContainer"),
				OpenVisualiserButton = m_ConfiguratorElement.Q<Button>(name: "openVisualiser"),
			};

			m_AgentConfigurator.OpenVisualiserButton.clickable.clicked += () =>
			{
				PuppeteerPlanVisualiserWindow.CreateWindow();
			};

			RegisterConfiguratorCallbacks();
		}

		protected override void UpdateConfigurator()
		{
			if (m_SelectedListItem is AgentListItem selectedAgentListItem)
			{
				EnableRightPanelContent();
				LazyInitConfigurator();

				PuppeteerAgent selectedAgent = selectedAgentListItem.GetAgent();
				if (selectedAgent == null)
				{
					return;
				}

				EnsureArchetypeSelectorIsInSync();
				m_AgentConfigurator.ArchetypeField.SetValueWithoutNotify(selectedAgent.ArchetypeGUID);
				m_AgentConfigurator.AgentName.Unbind();
				m_AgentConfigurator.AgentName.Bind(new SerializedObject(selectedAgent.gameObject));
				AddPrefabIcon(m_AgentConfigurator.PrefabIcon, selectedAgent);

				UpdateWorkingMemoryDisplay(selectedAgent);
				UpdatePlanDisplay(selectedAgent);
			}
		}

		private void AgentArchetypeChanged(Guid _oldGuid, Guid _newGuid)
		{
			EnsureArchetypeSelectorIsInSync();

			m_AgentConfigurator.ArchetypeField.SetValueWithoutNotify(_newGuid);
		}

		private void ArchetypeFieldChangedEvent(ChangeEvent<Guid> _event)
		{
			PuppeteerAgent selectedAgent = (m_SelectedListItem as AgentListItem)?.GetAgent();
			if (selectedAgent == null)
			{
				return;
			}

			selectedAgent.ArchetypeGUID = _event.newValue;
			EditorUtility.SetDirty(selectedAgent);
		}

		private AgentListItem CreateAgentListItem(PuppeteerAgent _agent)
		{
			AgentListItem item = new AgentListItem(_agent);
			item.OnMouseDown += UpdateSelectedListItem;
			m_ListItems.Add(item);

			_agent.OnPuppeteerAgentDestroy += PuppeteerAgentDestroy;

			return item;
		}

		private void EditorPlayModeStateChanged(PlayModeStateChange _stateChange)
		{
			switch (_stateChange)
			{
				case PlayModeStateChange.EnteredEditMode:
				case PlayModeStateChange.EnteredPlayMode:
					if (m_IsOpen)
					{
						CloseView();
						ReloadAgentListItems();
						m_SelectedListItem = null;
						OpenView(m_LastSelectedAgentObjectID);
					}
					else
					{
						ReloadAgentListItems();
					}
					break;

				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.ExitingPlayMode:
					break;

				default:
					break;
			}
		}

		private void EnsureArchetypeSelectorIsInSync()
		{
			PuppeteerAgent selectedAgent = (m_SelectedListItem as AgentListItem)?.GetAgent();
			if (selectedAgent == null)
			{
				return;
			}

			ref var archetypeField = ref m_AgentConfigurator.ArchetypeField;

			if (!archetypeField.IsInSync())
			{
				PuppeteerEditorHelper.ReplaceArchetypeSelectorField(ref archetypeField, selectedAgent.ArchetypeGUID, ArchetypeFieldChangedEvent);
			}
		}

		private void ListItemDeselected(ListItem _listItem)
		{
			if (_listItem is AgentListItem deselectedAgentListItem)
			{
				PuppeteerAgent deselectedAgent = deselectedAgentListItem.GetAgent();

				deselectedAgent.OnWorkingMemoryChanged -= SelectedAgentWorkingMemoryChanged;
				deselectedAgent.OnArchetypeChanged -= AgentArchetypeChanged;
			}
		}

		private void ListItemSelectedOrRemoved(ListItem _listItem)
		{
			if (_listItem == null)
			{
				m_OnUpdateSerialisedLastSelected?.Invoke(0);
				m_LastSelectedAgentObjectID = 0;
			}
			else
			{
				PuppeteerAgent selectedAgent = (_listItem as AgentListItem).GetAgent();

				int selectedInstanceID = selectedAgent.gameObject.GetInstanceID();
				m_LastSelectedAgentObjectID = selectedInstanceID;

				m_OnUpdateSerialisedLastSelected?.Invoke(selectedInstanceID);

				selectedAgent.OnArchetypeChanged += AgentArchetypeChanged;
			}
		}

		private void OpenView(int _lastOpenedAgentObjectID)
		{
			base.OpenView(null);

			m_ListItemScrollViewHeader.text = "Agents in Scene";
			m_ListItemScrollViewHeaderIcon.image = PuppeteerEditorResourceLoader.AgentIcon32.texture;

			m_AddButton.visible = false;
			m_RightPanelContent.Q<Label>(name: "GUIDLabel").text = string.Empty;

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				m_ListItemScrollView.Add(m_ListItems[i]);
			}

			if (m_SelectedListItem != null)
			{
				UpdateConfigurator();
			}
			else if (_lastOpenedAgentObjectID != 0)
			{
				TryOpenEntry(_lastOpenedAgentObjectID);
			}
			else
			{
				DisableRightPanelContent();
			}

			OnListItemDeselected = ListItemDeselected;
			RegisterWindowCallbacks();
		}

		private void PuppeteerAgentDestroy(PuppeteerAgent _agent)
		{
			ListItem foundListItem = m_ListItems.Find(_entry => (_entry as AgentListItem).GetAgent() == _agent);
			if (foundListItem != null)
			{
				foundListItem.RemoveFromHierarchy();
				m_ListItems.Remove(foundListItem);
			}
		}

		private void RegisterConfiguratorCallbacks()
		{
			m_AgentConfigurator.ArchetypeField.RegisterCallback<ChangeEvent<System.Guid>>(ArchetypeFieldChangedEvent);

			m_AgentConfigurator.ArchetypeField.RegisterCallback<MouseEnterEvent>(_event =>
			{
				EnsureArchetypeSelectorIsInSync();
			});

			m_AgentConfigurator.AgentName.RegisterCallback<ChangeEvent<string>>(_change =>
			{
				if (m_SelectedListItem is AgentListItem selectedAgentListItem)
				{
					m_SelectedListItem.ChangeText(_change.newValue);
				}
			});
		}

		private void RegisterWindowCallbacks()
		{
			m_RootElement.RegisterCallback<MouseEnterEvent>(UpdateAgentsOnEnterWindow);
		}

		private void ReloadAgentListItems()
		{
			m_ListItems.Clear();

			HashSet<PuppeteerAgent> puppets = PuppeteerManager.Instance.GetAgents();

			if (puppets.Count > 0)
			{
				foreach (var puppet in PuppeteerManager.Instance.GetAgents().OrderBy(_entry => _entry.name))
				{
					CreateAgentListItem(puppet);
				}
			}
		}

		private void SelectedAgentWorkingMemoryChanged(WorldState<string, object> _worldState)
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

			PuppeteerAgent selectedAgent = (m_SelectedListItem as AgentListItem)?.GetAgent();
			UpdatePlanDisplay(selectedAgent);
		}

		private void UnregisterWindowCallbacks()
		{
			m_RootElement.UnregisterCallback<MouseEnterEvent>(UpdateAgentsOnEnterWindow);
		}

		private void UpdateAgentsOnEnterWindow(MouseEnterEvent _enterEvent)
		{
			if (m_SelectedListItem != null)
			{
				UpdateConfigurator();
			}
			else
			{
				DisableRightPanelContent();
			}

			OnListItemDeselected = ListItemDeselected;
		}

		private void UpdatePlanDisplay(PuppeteerAgent _selectedAgent)
		{
			m_AgentConfigurator.PlanContainer.Clear();

			if (_selectedAgent.GetActiveAction() != null)
			{
				m_AgentConfigurator.PlanContainer.Add(new PlanItem(PlanItem.PlanItemType.ActiveAction, _selectedAgent.GetActiveAction().GetLabel()));
			}

			Plan<string, object> plan = _selectedAgent.GetPlan();
			if (plan != null)
			{
				foreach (var item in _selectedAgent.GetPlan())
				{
					m_AgentConfigurator.PlanContainer.Add(new PlanItem(PlanItem.PlanItemType.InactiveAction, item.GetLabel()));
				}

				PuppeteerGoal activeGoal = _selectedAgent.GetActiveGoal();
				if (activeGoal != null)
				{
					var goalDesc = PuppeteerManager.Instance.GetGoalDescription(activeGoal.DescriptionGUID);
					m_AgentConfigurator.PlanContainer.Add(new PlanItem(PlanItem.PlanItemType.ActiveGoal, goalDesc.DisplayName));
				}
			}
		}

		private void UpdateWorkingMemoryDisplay(PuppeteerAgent _selectedAgent)
		{
			m_AgentConfigurator.WorkingMemoryContainer.Clear();
			m_WorkingMemoryListItems.Clear();

			foreach (KeyValuePair<string, object> memoryEntry in _selectedAgent.GetWorkingMemory())
			{
				// #performancePotential: Cache working memory items instead of clearing and creating new ones

				WorkingMemoryItem workingMemoryItem = new WorkingMemoryItem(memoryEntry);

				m_AgentConfigurator.WorkingMemoryContainer.Add(workingMemoryItem);
				m_WorkingMemoryListItems.Add(workingMemoryItem);
			}

			_selectedAgent.OnWorkingMemoryChanged += SelectedAgentWorkingMemoryChanged;
		}

		private readonly Action<int> m_OnUpdateSerialisedLastSelected;
		private readonly List<WorkingMemoryItem> m_WorkingMemoryListItems = new List<WorkingMemoryItem>();
		private AgentConfigurator m_AgentConfigurator = null;
		private int m_LastSelectedAgentObjectID = 0;
		private List<Editor> m_PreviewEditors;

		private class AgentConfigurator
		{
			public TextField AgentName;
			public ArchetypeSelectorField ArchetypeField;
			public Button OpenVisualiserButton;
			public VisualElement PlanContainer;
			public VisualElement PrefabIcon;
			public VisualElement WorkingMemoryContainer;
		}
	}
}