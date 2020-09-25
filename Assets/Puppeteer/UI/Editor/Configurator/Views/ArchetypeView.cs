using Puppeteer.Core;
using Puppeteer.Core.Configuration;
using Puppeteer.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class ArchetypeView : PuppeteerView
	{
		public ArchetypeView(VisualElement _rootElement, VisualElement _leftPanel, VisualElement _rightPanel, Action<Guid> _updateLastSelectedCallback)
			: base(_rootElement, _leftPanel, _rightPanel)
		{
			m_Label = "Archetypes";
			tooltip = "Create and configure the archetypes that define sets of goals, actions, and sensors.";

			var visualTreeAsset = Resources.Load<VisualTreeAsset>("LayoutArchetypeConfigurator");
			visualTreeAsset.CloneTree(m_ConfiguratorElement);

			m_ConfiguratorElement.name = "configuratorContent";

			PuppeteerManager.Instance.LoadDescriptionsOfType<ArchetypeDescription>();

			m_SerialisedArchetypes = PuppeteerManager.Instance.GetArchetypeDescriptions();

			for (int i = 0; i < m_SerialisedArchetypes.Count; ++i)
			{
				CreateArchetypeListItem(m_SerialisedArchetypes[i]);
			}

			m_ContextMenuScrollView = new ContextualMenuManipulator(_menubuilder =>
			{
				_menubuilder.menu.AppendAction("Create New Archetype", _dropDownMenuAction => { AddNewArchetypeListItem(); }, DropdownMenuAction.Status.Normal);
			});

			m_ListItemInOtherTabDeleted = new Dictionary<Type, Action<ListItem>>
			{
				{typeof(GoalListItem), OtherTabGoalListItemDeleted},
				{typeof(ActionListItem), OtherTabActionListItemDeleted},
				{typeof(SensorListItem), OtherTabSensorListItemDeleted},
			};

			m_OnUpdateSerialisedLastSelected = _updateLastSelectedCallback;
			OnListItemSelectedOrRemoved = ListItemSelectedOrRemoved;
		}

		public override void ClearSelection()
		{
			if (m_SelectedContentListItem != null)
			{
				UpdateSelectedContentListItem(null);
			}
			else
			{
				base.ClearSelection();
			}
		}

		public override void CloseView()
		{
			base.CloseView();
			m_AddButton.clickable.clicked -= AddNewArchetypeListItem;

			m_SearchPopup?.RemoveFromHierarchy();

			m_ListItemScrollView.RemoveManipulator(m_ContextMenuScrollView);
		}

		public override void DeleteSelection()
		{
			if (m_SelectedContentListItem != null)
			{
				DeleteContentListItem(m_SelectedContentListItem);
			}
			else
			{
				base.DeleteSelection();
			}
		}

		public override void DuplicateSelection()
		{
			if (m_SelectedListItem != null)
			{
				AddNewArchetypeListItem((m_SelectedListItem as ArchetypeListItem).GetDescription());
			}
		}

		public override void EnterSelection()
		{
			if (m_SearchPopup != null)
			{
				(m_SearchPopup as SearchPopup<GoalDescription>)?.ChooseSelected();
				(m_SearchPopup as SearchPopup<ActionDescription>)?.ChooseSelected();
				(m_SearchPopup as SearchPopup<SensorDescription>)?.ChooseSelected();
			}
			else if (m_SelectedContentListItem != null)
			{
				OpenOtherTabEntryRelatedToListItem(m_SelectedContentListItem);
			}
		}

		public override void MoveSelection(MoveDirection _direction)
		{
			if (m_SearchPopup != null)
			{
				(m_SearchPopup as SearchPopup<GoalDescription>)?.MoveSelection(_direction);
				(m_SearchPopup as SearchPopup<ActionDescription>)?.MoveSelection(_direction);
				(m_SearchPopup as SearchPopup<SensorDescription>)?.MoveSelection(_direction);
			}
			else if (m_SelectedContentListItem != null)
			{
				int scrollViewIndex = m_ArchetypeConfigurator.ScrollViewsSortedByXPosition.IndexOf(m_SelectedContentListItem.parent as ScrollView);

				if (_direction == MoveDirection.Left && scrollViewIndex > 0)
				{
					var leftNeighbour = m_ArchetypeConfigurator.ScrollViewsSortedByXPosition[--scrollViewIndex];
					if (leftNeighbour?.childCount > 0)
					{
						UpdateSelectedContentListItem(leftNeighbour[0] as ListItem);
					}
				}
				else if (_direction == MoveDirection.Right && scrollViewIndex < m_ArchetypeConfigurator.ScrollViewsSortedByXPosition.Count - 1)
				{
					var rightNeighbour = m_ArchetypeConfigurator.ScrollViewsSortedByXPosition[++scrollViewIndex];
					if (rightNeighbour?.childCount > 0)
					{
						UpdateSelectedContentListItem(rightNeighbour[0] as ListItem);
					}
				}
				else
				{
					SelectNeighbourContentListItem(_direction);
				}
			}
			else
			{
				base.MoveSelection(_direction);
			}
		}

		public override void OpenView(SerialisedConfiguratorState _serialisedConfiguratorStates)
		{
			base.OpenView(_serialisedConfiguratorStates);

			m_ListItemScrollViewHeader.text = "Available Archetypes";
			m_ListItemScrollViewHeaderIcon.image = PuppeteerEditorResourceLoader.ArchetypeIcon32.texture;

			m_AddButton.clickable.clicked += AddNewArchetypeListItem;
			m_AddButton.tooltip = "Create a new archetype.";

			m_ListItemScrollView.AddManipulator(m_ContextMenuScrollView);

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				m_ListItemScrollView.Add(m_ListItems[i]);
			}

			if (m_SelectedListItem != null)
			{
				UpdateConfigurator();
			}
			else if (!_serialisedConfiguratorStates.LastOpenedArchetype.Equals(Guid.Empty))
			{
				TryOpenEntry(_serialisedConfiguratorStates.LastOpenedArchetype.Value);
			}
			else
			{
				DisableRightPanelContent();
			}
		}

		public override void RegisterTabViewCallbacks(TabView _tabView)
		{
			_tabView.OnTabListItemDeleted += OtherTabListItemDeleted;
		}

		public override void SaveAllChanges()
		{
			bool shouldSaveToFile = PuppeteerEditorHelper.RemoveDeletedItems<ArchetypeListItem, ArchetypeDescription>(m_SerialisedArchetypes, m_ListItems) > 0;

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				shouldSaveToFile |= PuppeteerEditorHelper.AddOrUpdateInList<ArchetypeListItem, ArchetypeDescription>(m_SerialisedArchetypes, m_ListItems[i]);
			}

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveArchetypes();

			if (successful)
			{
				for (int i = 0; i < m_ListItems.Count; ++i)
				{
					m_ListItems[i].MarkUnsavedChanges(false);
					TryClearUnsavedMarker();
				}
			}
		}

		public override void SaveSelectedChange()
		{
			bool shouldSaveToFile = PuppeteerEditorHelper.AddOrUpdateInList<ArchetypeListItem, ArchetypeDescription>(m_SerialisedArchetypes, m_SelectedListItem);

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveArchetypes();

			if (successful)
			{
				m_SelectedListItem.MarkUnsavedChanges(false);
				TryClearUnsavedMarker();
			}
		}

		public override bool TryOpenEntry(Guid _guid)
		{
			var matchingItem = m_ListItems.Find(_entry => (_entry as ArchetypeListItem).GetDescription().GUID == _guid);

			if (matchingItem != null)
			{
				UpdateSelectedListItem(matchingItem);
				return true;
			}
			else
			{
				return base.TryOpenEntry(_guid);
			}
		}

		protected override void LazyInitConfigurator()
		{
			if (m_ArchetypeConfigurator != null)
			{
				return;
			}

			m_ArchetypeConfigurator = new ArchetypeConfigurator
			{
				DisplayName = m_ConfiguratorElement.Q<TextField>(name: "displayName"),
				GoalAddButton = m_ConfiguratorElement.Q<Button>(name: "goalAddButton"),
				ActionAddButton = m_ConfiguratorElement.Q<Button>(name: "actionAddButton"),
				SensorAddButton = m_ConfiguratorElement.Q<Button>(name: "sensorAddButton"),
				GoalScrollView = m_ConfiguratorElement.Q<ScrollView>(name: "goalScrollView"),
				ActionScrollView = m_ConfiguratorElement.Q<ScrollView>(name: "actionScrollView"),
				SensorScrollView = m_ConfiguratorElement.Q<ScrollView>(name: "sensorScrollView"),
				GUIDLabel = m_RightPanelContent.Q<Label>(name: "GUIDLabel"),
			};

			m_ArchetypeConfigurator.ScrollViewsSortedByXPosition = new List<ScrollView>
			{
				m_ArchetypeConfigurator.GoalScrollView,
				m_ArchetypeConfigurator.ActionScrollView,
				m_ArchetypeConfigurator.SensorScrollView,
			};

			// just in case the scrollViews are reordered in the UXML file.
			m_ArchetypeConfigurator.ScrollViewsSortedByXPosition = m_ArchetypeConfigurator.ScrollViewsSortedByXPosition.OrderBy(_entry => _entry.layout.x).ToList();

			RegisterConfiguratorCallbacks();
		}

		protected override void UpdateConfigurator()
		{
			if (m_SelectedListItem is ArchetypeListItem selectedArchetypeListItem)
			{
				EnableRightPanelContent();
				LazyInitConfigurator();

				ArchetypeDescription selectedDescription = selectedArchetypeListItem.GetDescription();

				m_ArchetypeConfigurator.GUIDLabel.text = selectedDescription.GUID.ToString();
				m_ArchetypeConfigurator.DisplayName.value = selectedDescription.DisplayName;

				m_ArchetypeConfigurator.DisplayName.RegisterCallback<FocusOutEvent>(_eventTarget =>
				{
					if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary((_eventTarget.target as TextField), ref (m_SelectedListItem as ArchetypeListItem).GetDescription().DisplayName))
					{
						m_SelectedListItem.ChangeText((_eventTarget.target as TextField).value);
						m_SelectedListItem.MarkUnsavedChanges(true);
						AddUnsavedMarker();
					}
				});

				m_SearchPopup?.RemoveFromHierarchy();
				m_SearchPopup = null;

				FillBasicListToVisualElement<GoalDescription>(selectedDescription.GoalGUIDs, m_ArchetypeConfigurator.GoalScrollView);
				FillBasicListToVisualElement<ActionDescription>(selectedDescription.ActionGUIDs, m_ArchetypeConfigurator.ActionScrollView);
				FillBasicListToVisualElement<SensorDescription>(selectedDescription.SensorGUIDs, m_ArchetypeConfigurator.SensorScrollView);
			}
		}

		private void AddNewArchetypeListItem()
		{
			ArchetypeDescription archetypeDescription = new ArchetypeDescription
			{
				DisplayName = "New Archetype",
				GoalGUIDs = new Guid[0],
				ActionGUIDs = new Guid[0],
				SensorGUIDs = new Guid[0],
			};

			AddNewArchetypeListItem(archetypeDescription);
		}

		private void AddNewArchetypeListItem(ArchetypeDescription _archetypeDescription)
		{
			ArchetypeDescription newArchetypeDescription = new ArchetypeDescription
			{
				GUID = Guid.NewGuid(),
				DisplayName = _archetypeDescription.DisplayName,
				GoalGUIDs = _archetypeDescription.GoalGUIDs,
				ActionGUIDs = _archetypeDescription.ActionGUIDs,
				SensorGUIDs = _archetypeDescription.SensorGUIDs,
			};

			ArchetypeListItem item = CreateArchetypeListItem(newArchetypeDescription);
			m_ListItemScrollView.Add(item);

			item.MarkUnsavedChanges(true);
			AddUnsavedMarker();

			UpdateSelectedListItem(item);
		}

		private void AddNewComponentListItem(ListItem _item)
		{
			m_SearchPopup?.RemoveFromHierarchy();
			m_SearchPopup = null;

			if (m_SelectedListItem is ArchetypeListItem selectedArchetypeListItem)
			{
				ArchetypeDescription selectedDescription = selectedArchetypeListItem.GetDescription();

				if (_item is BasicListItem<GoalDescription> goalItem)
				{
					var goalDescription = goalItem.GetDescription();

					PuppeteerEditorHelper.Append(ref selectedDescription.GoalGUIDs, goalDescription.GUID);

					m_ArchetypeConfigurator.GoalScrollView.Add(CreateBasicListItemOfType<GoalDescription>(goalDescription.GUID));
				}
				else if (_item is BasicListItem<ActionDescription> actionItem)
				{
					var actionDescription = actionItem.GetDescription();

					PuppeteerEditorHelper.Append(ref selectedDescription.ActionGUIDs, actionDescription.GUID);

					m_ArchetypeConfigurator.ActionScrollView.Add(CreateBasicListItemOfType<ActionDescription>(actionDescription.GUID));

					selectedDescription.ActionGUIDs.Append(actionItem.GetDescription().GUID);
				}
				else if (_item is BasicListItem<SensorDescription> sensorItem)
				{
					var sensorDescription = sensorItem.GetDescription();

					PuppeteerEditorHelper.Append(ref selectedDescription.SensorGUIDs, sensorDescription.GUID);

					m_ArchetypeConfigurator.SensorScrollView.Add(CreateBasicListItemOfType<SensorDescription>(sensorDescription.GUID));

					selectedDescription.SensorGUIDs.Append(sensorItem.GetDescription().GUID);
				}
			}

			m_SelectedListItem.MarkUnsavedChanges(true);
			AddUnsavedMarker();
		}

		private ArchetypeListItem CreateArchetypeListItem(ArchetypeDescription _archetypeDescription)
		{
			ValidateAndFixDescription(ref _archetypeDescription);
			ArchetypeListItem item = new ArchetypeListItem(_archetypeDescription);
			item.OnMouseDown += UpdateSelectedListItem;
			item.OnDelete += DeleteListItem;
			item.OnDuplicate += _item => AddNewArchetypeListItem((_item as ArchetypeListItem).GetDescription());
			m_ListItems.Add(item);

			return item;
		}

		private BasicListItem<T> CreateBasicListItemOfType<T>(Guid _guid) where T : BasicDescription, new()
		{
			T description = PuppeteerManager.Instance.GetDescriptionOfType<T>(_guid);
			if (description == null)
			{
				description = new T
				{
					GUID = _guid,
					DisplayName = "Invalid GUID"
				};
			}

			var newBasicListItem = new BasicListItem<T>(_description: description, _useDuplicateManipulator: false);
			newBasicListItem.OnMouseDown += UpdateSelectedContentListItem;
			newBasicListItem.OnDelete += DeleteContentListItem;
			newBasicListItem.tooltip = m_BasicListItemTooltipText;

			return newBasicListItem;
		}

		private void DeleteContentListItem(ListItem _contentListItem)
		{
			if (m_SelectedListItem is ArchetypeListItem selectedArchetypeListItem)
			{
				if (_contentListItem == m_SelectedContentListItem)
				{
					SelectNeighbourContentListItemIfNeeded(_contentListItem);
				}
				_contentListItem.RemoveFromHierarchy();

				if (_contentListItem is BasicListItem<GoalDescription> goalContentListItem)
				{
					PuppeteerEditorHelper.Remove(ref selectedArchetypeListItem.GetDescription().GoalGUIDs, goalContentListItem.GetDescription().GUID);
				}
				else if (_contentListItem is BasicListItem<ActionDescription> actionContentListItem)
				{
					PuppeteerEditorHelper.Remove(ref selectedArchetypeListItem.GetDescription().ActionGUIDs, actionContentListItem.GetDescription().GUID);
				}
				else if (_contentListItem is BasicListItem<SensorDescription> sensorContentListItem)
				{
					PuppeteerEditorHelper.Remove(ref selectedArchetypeListItem.GetDescription().SensorGUIDs, sensorContentListItem.GetDescription().GUID);
				}

				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();
			}
		}

		private void DeleteGuidFromContainerIfNeeded(ref ArchetypeListItem _archetypeListItem, ref Guid[] _container, Guid _deletedGuid)
		{
			if (_container.Contains(_deletedGuid))
			{
				PuppeteerEditorHelper.Remove(ref _container, _deletedGuid);
				_archetypeListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();
			}
		}

		private void FillBasicListToVisualElement<T>(Guid[] _content, VisualElement _targetList) where T : BasicDescription, new()
		{
			var sortedContent = _content.OrderBy(_entry =>
			{
				return PuppeteerManager.Instance.GetDescriptionOfType<T>(_entry).DisplayName;
			}).ToArray();

			_targetList.Clear();

			for (int i = 0; i < sortedContent.Length; ++i)
			{
				_targetList.Add(CreateBasicListItemOfType<T>(sortedContent[i]));
			}
		}

		private List<string> GetRelevantWorldStatesForSelectedContentListItem()
		{
			List<string> relevantWorldStates = new List<string>();

			foreach (var relatedListItem in m_RelatedContentListItems)
			{
				relatedListItem.RemoveFromClassList("related");
			}
			m_RelatedContentListItems.Clear();

			if (m_SelectedContentListItem is BasicListItem<GoalDescription> selectedGoalListItem)
			{
				foreach (var goalPart in selectedGoalListItem.GetDescription().GoalParts)
				{
					relevantWorldStates.Add(goalPart.Key);
				}
			}
			else if (m_SelectedContentListItem is BasicListItem<ActionDescription> selectedActionListItem)
			{
				foreach (var precondition in selectedActionListItem.GetDescription().Preconditions)
				{
					relevantWorldStates.Add(precondition.Key);
				}
				foreach (var effect in selectedActionListItem.GetDescription().Effects)
				{
					relevantWorldStates.Add(effect.Key);
				}
			}
			else if (m_SelectedContentListItem is BasicListItem<SensorDescription> selectedSensorListItem)
			{
				relevantWorldStates.Add(selectedSensorListItem.GetDescription().ManagedWorldState);
			}

			return relevantWorldStates;
		}

		private void ListItemSelectedOrRemoved(ListItem _listItem)
		{
			if (_listItem == null)
			{
				m_OnUpdateSerialisedLastSelected?.Invoke(Guid.Empty);
			}
			else
			{
				m_OnUpdateSerialisedLastSelected?.Invoke((_listItem as ArchetypeListItem).GetDescription().GUID);
			}
		}

		private void OpenOtherTabEntryRelatedToListItem(ListItem _listItem)
		{
			if (_listItem is BasicListItem<GoalDescription> selectedGoalListItem)
			{
				(parent as TabView).OpenEntryInTabOfType<GoalView>(selectedGoalListItem.GetDescription().GUID);
			}
			else if (_listItem is BasicListItem<ActionDescription> selectedActionListItem)
			{
				(parent as TabView).OpenEntryInTabOfType<ActionView>(selectedActionListItem.GetDescription().GUID);
			}
			else if (_listItem is BasicListItem<SensorDescription> selectedSensorListItem)
			{
				(parent as TabView).OpenEntryInTabOfType<SensorView>(selectedSensorListItem.GetDescription().GUID);
			}
		}

		private void OpenSelectorPopup<TDesc>(List<TDesc> _entries, Button _button) where TDesc : BasicDescription
		{
			if (m_SearchPopup != null)
			{
				m_SearchPopup.RemoveFromHierarchy();
			}

			Vector2 popupPosition = _button.LocalToParent(_button.clickable.lastMousePosition, m_ConfiguratorElement);

			Vector2 minMaxX = new Vector2(m_ConfiguratorElement.layout.xMin, m_ConfiguratorElement.layout.xMax);

			m_SearchPopup = new SearchPopup<TDesc>(m_RootElement, _entries, popupPosition, minMaxX);

			m_ConfiguratorElement.Add(m_SearchPopup);

			if (m_SearchPopup is SearchPopup<TDesc> searchPopup)
			{
				searchPopup.OnListItemClicked += AddNewComponentListItem;
			}
		}

		private void OtherTabActionListItemDeleted(ListItem _deletedListItem)
		{
			if (_deletedListItem is ActionListItem deletedActionListItem)
			{
				for (int i = 0; i < m_ListItems.Count; ++i)
				{
					if (m_ListItems[i] is ArchetypeListItem archetypeListItem)
					{
						DeleteGuidFromContainerIfNeeded(ref archetypeListItem, ref archetypeListItem.GetDescription().ActionGUIDs, deletedActionListItem.GetDescription().GUID);
					}
				}
			}
		}

		private void OtherTabGoalListItemDeleted(ListItem _deletedListItem)
		{
			if (_deletedListItem is GoalListItem deletedGoalListItem)
			{
				for (int i = 0; i < m_ListItems.Count; ++i)
				{
					if (m_ListItems[i] is ArchetypeListItem archetypeListItem)
					{
						DeleteGuidFromContainerIfNeeded(ref archetypeListItem, ref archetypeListItem.GetDescription().GoalGUIDs, deletedGoalListItem.GetDescription().GUID);
					}
				}
			}
		}

		private void OtherTabListItemDeleted(ListItem _deletedListItem)
		{
			if (m_ListItemInOtherTabDeleted.TryGetValue(_deletedListItem.GetType(), out var deleteAction))
			{
				deleteAction.Invoke(_deletedListItem);
			}
		}

		private void OtherTabSensorListItemDeleted(ListItem _deletedListItem)
		{
			if (_deletedListItem is SensorListItem deletedSensorListItem)
			{
				for (int i = 0; i < m_ListItems.Count; ++i)
				{
					if (m_ListItems[i] is ArchetypeListItem archetypeListItem)
					{
						DeleteGuidFromContainerIfNeeded(ref archetypeListItem, ref archetypeListItem.GetDescription().SensorGUIDs, deletedSensorListItem.GetDescription().GUID);
					}
				}
			}
		}

		private void RegisterConfiguratorCallbacks()
		{
			m_ArchetypeConfigurator.GoalAddButton.clickable.clicked += () =>
			{
				var goals = PuppeteerManager.Instance.GetGoalDescriptions();

				var filteredList = goals.Where(_entry =>
				{
					var selectedDescription = (m_SelectedListItem as ArchetypeListItem).GetDescription();
					return !selectedDescription.GoalGUIDs.Contains(_entry.GUID);
				}).ToList();

				OpenSelectorPopup(filteredList, m_ArchetypeConfigurator.GoalAddButton);
			};

			m_ArchetypeConfigurator.ActionAddButton.clickable.clicked += () =>
			{
				var actions = PuppeteerManager.Instance.GetActionDescriptions();

				var filteredList = actions.Where(_entry =>
				{
					var selectedDescription = (m_SelectedListItem as ArchetypeListItem).GetDescription();
					return !selectedDescription.ActionGUIDs.Contains(_entry.GUID);
				}).ToList();

				OpenSelectorPopup(filteredList, m_ArchetypeConfigurator.ActionAddButton);
			};

			m_ArchetypeConfigurator.SensorAddButton.clickable.clicked += () =>
			{
				var sensors = PuppeteerManager.Instance.GetSensorDescriptions();

				var filteredList = sensors.Where(_entry =>
				{
					var selectedDescription = (m_SelectedListItem as ArchetypeListItem).GetDescription();
					return !selectedDescription.SensorGUIDs.Contains(_entry.GUID);
				}).ToList();

				OpenSelectorPopup(filteredList, m_ArchetypeConfigurator.SensorAddButton);
			};
		}

		private bool SelectNeighbourContentListItem(MoveDirection _direction)
		{
			if (_direction != MoveDirection.Up && _direction != MoveDirection.Down)
			{
				return false;
			}

			VisualElement containingScrollView = m_SelectedContentListItem.parent;

			int index = containingScrollView.IndexOf(m_SelectedContentListItem);
			if (index > 0 && _direction == MoveDirection.Up)
			{
				UpdateSelectedContentListItem(containingScrollView[--index] as ListItem);
				return true;
			}
			else if (index < containingScrollView.childCount - 1 && _direction == MoveDirection.Down)
			{
				UpdateSelectedContentListItem(containingScrollView[++index] as ListItem);
				return true;
			}

			return false;
		}

		private void SelectNeighbourContentListItemIfNeeded(ListItem _contentListItem)
		{
			if (_contentListItem != m_SelectedContentListItem)
			{
				return;
			}

			bool neighbourSelected = SelectNeighbourContentListItem(MoveDirection.Up);
			if (!neighbourSelected)
			{
				neighbourSelected |= SelectNeighbourContentListItem(MoveDirection.Down);
			}

			if (!neighbourSelected)
			{
				m_SelectedContentListItem = null;
			}
		}

		private bool TryWeakSelectIfActionListItem(VisualElement _item, ref List<string> _relevantWorldStates)
		{
			if (_item is BasicListItem<ActionDescription> actionListItem)
			{
				foreach (var precondition in actionListItem.GetDescription().Preconditions)
				{
					if (_relevantWorldStates.Find(_entry => _entry.Equals(precondition.Key)) != null)
					{
						m_RelatedContentListItems.Add(actionListItem);
						actionListItem.AddToClassList("related");
						return true;
					}
				}

				foreach (var effect in actionListItem.GetDescription().Effects)
				{
					if (_relevantWorldStates.Find(_entry => _entry.Equals(effect.Key)) != null)
					{
						m_RelatedContentListItems.Add(actionListItem);
						actionListItem.AddToClassList("related");
						return true;
					}
				}
			}

			return false;
		}

		private bool TryWeakSelectIfGoalListItem(VisualElement _item, ref List<string> _relevantWorldStates)
		{
			if (_item is BasicListItem<GoalDescription> goalListItem)
			{
				foreach (var goalPart in goalListItem.GetDescription().GoalParts)
				{
					if (_relevantWorldStates.Find(_entry => _entry.Equals(goalPart.Key)) != null)
					{
						m_RelatedContentListItems.Add(goalListItem);
						goalListItem.AddToClassList("related");
						return true;
					}
				}
			}

			return false;
		}

		private bool TryWeakSelectIfSensorListItem(VisualElement _item, ref List<string> _relevantWorldStates)
		{
			if (_item is BasicListItem<SensorDescription> sensorListItem)
			{
				string managedState = sensorListItem.GetDescription().ManagedWorldState;
				if (_relevantWorldStates.Find(_entry => _entry.Equals(managedState)) != null)
				{
					m_RelatedContentListItems.Add(sensorListItem);
					sensorListItem.AddToClassList("related");
					return true;
				}
			}

			return false;
		}

		private void UpdateSelectedContentListItem(ListItem _listItem)
		{
			m_SearchPopup?.RemoveFromHierarchy();

			if (m_SelectedContentListItem != null)
			{
				m_SelectedContentListItem.RemoveFromClassList("selected");
				if (_listItem == m_SelectedContentListItem)
				{
					OpenOtherTabEntryRelatedToListItem(_listItem);
					return;
				}
			}
			m_SelectedContentListItem = _listItem;
			m_SelectedContentListItem?.AddToClassList("selected");

			WeakSelectRelatedItemsInOtherLists();
		}

		private void ValidateAndFixDescription(ref ArchetypeDescription _archetypeDescription)
		{
			if (_archetypeDescription.GoalGUIDs == null)
			{
				_archetypeDescription.GoalGUIDs = new Guid[0];
			}

			if (_archetypeDescription.ActionGUIDs == null)
			{
				_archetypeDescription.ActionGUIDs = new Guid[0];
			}

			if (_archetypeDescription.SensorGUIDs == null)
			{
				_archetypeDescription.SensorGUIDs = new Guid[0];
			}
		}

		private void WeakSelectRelatedItemsInOtherLists()
		{
			var relevantWorldStates = GetRelevantWorldStatesForSelectedContentListItem();

			foreach (ScrollView scrollView in m_ArchetypeConfigurator.ScrollViewsSortedByXPosition)
			{
				foreach (VisualElement item in scrollView.Children())
				{
					if (item == m_SelectedContentListItem)
					{
						continue;
					}

					if (TryWeakSelectIfGoalListItem(item, ref relevantWorldStates))
					{
						continue;
					}

					if (TryWeakSelectIfActionListItem(item, ref relevantWorldStates))
					{
						continue;
					}

					TryWeakSelectIfSensorListItem(item, ref relevantWorldStates);
				}
			}
		}

		private const string m_BasicListItemTooltipText = "Select to see related goals, actions, and sensors. Click twice to open its configuration.";
		private readonly ContextualMenuManipulator m_ContextMenuScrollView;

		private readonly Dictionary<Type, Action<ListItem>> m_ListItemInOtherTabDeleted;
		private readonly Action<Guid> m_OnUpdateSerialisedLastSelected;
		private readonly List<ListItem> m_RelatedContentListItems = new List<ListItem>();
		private readonly List<ArchetypeDescription> m_SerialisedArchetypes;

		private ArchetypeConfigurator m_ArchetypeConfigurator = null;
		private VisualElement m_SearchPopup;
		private ListItem m_SelectedContentListItem = null;

		private class ArchetypeConfigurator
		{
			public Button ActionAddButton;
			public ScrollView ActionScrollView;
			public TextField DisplayName;
			public Button GoalAddButton;
			public ScrollView GoalScrollView;
			public Label GUIDLabel;
			public List<ScrollView> ScrollViewsSortedByXPosition;
			public Button SensorAddButton;
			public ScrollView SensorScrollView;
		}
	}
}