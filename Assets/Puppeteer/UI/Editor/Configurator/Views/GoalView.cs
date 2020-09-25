using Puppeteer.Core;
using Puppeteer.Core.Configuration;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class GoalView : PuppeteerView
	{
		public GoalView(VisualElement _rootElement, VisualElement _leftPanel, VisualElement _rightPanel, Action<Guid> _updateLastSelectedCallback)
			: base(_rootElement, _leftPanel, _rightPanel)
		{
			m_Label = "Goals";
			tooltip = "Create and configure the goals that agents want to achieve.";

			var visualTreeAsset = Resources.Load<VisualTreeAsset>("LayoutGoalConfigurator");
			visualTreeAsset.CloneTree(m_ConfiguratorElement);

			PuppeteerManager.Instance.LoadDescriptionsOfType<GoalDescription>();

			m_SerialisedGoals = PuppeteerManager.Instance.GetGoalDescriptions();

			for (int i = 0; i < m_SerialisedGoals.Count; ++i)
			{
				CreateGoalListItem(m_SerialisedGoals[i]);
			}

			m_ContextMenuScrollView = new ContextualMenuManipulator(_menubuilder =>
			{
				_menubuilder.menu.AppendAction("Create New Goal", _dropDownMenuAction => { AddNewGoalListItem(); }, DropdownMenuAction.Status.Normal);
			});

			m_OnUpdateSerialisedLastSelected = _updateLastSelectedCallback;
			OnListItemSelectedOrRemoved = ListItemSelectedOrRemoved;
		}

		public override void AddNewBasedOnSelection()
		{
			if (m_SelectedGoalPartItem != null)
			{
				AddNewGoalPartItem();
			}
			else if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				AddNewUtilityItem();
			}
			else if (m_SelectedListItem != null)
			{
				AddNewGoalListItem();
			}
		}

		public override void ClearSelection()
		{
			if (m_SelectedGoalPartItem != null)
			{
				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, null);
			}
			else if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				UpdateSelectedUtilityItem(null);
			}
			else
			{
				base.ClearSelection();
			}
		}

		public override void CloseView()
		{
			base.CloseView();
			m_AddButton.clickable.clicked -= AddNewGoalListItem;

			m_ListItemScrollView.RemoveManipulator(m_ContextMenuScrollView);
		}

		public override void DeleteSelection()
		{
			if (m_SelectedGoalPartItem != null)
			{
				PuppeteerEditorHelper.DeleteWorldStateItem(this, m_SelectedListItem as GoalListItem,
					ref (m_SelectedListItem as GoalListItem).GetDescription().GoalParts, m_SelectedGoalPartItem, SelectNeighbourGoalPartItemIfNeeded);
			}
			else if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				DeleteUtilityItem(m_SelectedWorkingMemoryUtilityItem);
			}
			else
			{
				base.DeleteSelection();
			}
		}

		public override void DuplicateSelection()
		{
			if (m_SelectedGoalPartItem != null)
			{
				AddNewGoalPartItem(new WorldStateDescription(m_SelectedGoalPartItem.GetWorldStateDescription()));
			}
			else if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				AddNewUtilityItem(new UtilityDescription(m_SelectedWorkingMemoryUtilityItem.GetUtilityDescription()));
			}
			else if (m_SelectedListItem != null)
			{
				AddNewGoalListItem((m_SelectedListItem as GoalListItem).GetDescription());
			}
		}

		public override void EnterSelection()
		{
			if (m_SelectedGoalPartItem != null)
			{
				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, null);
			}
			else if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				UpdateSelectedUtilityItem(m_SelectedWorkingMemoryUtilityItem);
			}
			else if (m_SelectedListItem != null)
			{
				if (m_GoalConfigurator != null && m_GoalConfigurator.GoalPartsContainer.childCount > 0)
				{
					PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, m_GoalConfigurator.GoalPartsContainer[0] as WorldStateItem);
				}
			}
		}

		public override void MoveSelection(MoveDirection _direction)
		{
			if (m_SelectedGoalPartItem != null)
			{
				SelectNeighbourGoalPartItem(_direction);
			}
			else if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				SelectNeighbourUtilityItem(_direction);
			}
			else
			{
				base.MoveSelection(_direction);
			}
		}

		public override void OpenView(SerialisedConfiguratorState _serialisedConfiguratorStates)
		{
			base.OpenView(_serialisedConfiguratorStates);

			m_ListItemScrollViewHeader.text = "Available Goals";
			m_ListItemScrollViewHeaderIcon.image = PuppeteerEditorResourceLoader.GoalIcon32.texture;

			m_AddButton.clickable.clicked += AddNewGoalListItem;
			m_AddButton.tooltip = "Create a new goal.";

			m_ListItemScrollView.AddManipulator(m_ContextMenuScrollView);

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				m_ListItemScrollView.Add(m_ListItems[i]);
			}

			if (m_SelectedListItem != null)
			{
				UpdateConfigurator();
			}
			else if (!_serialisedConfiguratorStates.LastOpenedGoal.Equals(Guid.Empty))
			{
				TryOpenEntry(_serialisedConfiguratorStates.LastOpenedGoal.Value);
			}
			else
			{
				DisableRightPanelContent();
			}
		}

		public override void SaveAllChanges()
		{
			bool shouldSaveToFile = PuppeteerEditorHelper.RemoveDeletedItems<GoalListItem, GoalDescription>(m_SerialisedGoals, m_ListItems) > 0;

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				shouldSaveToFile |= PuppeteerEditorHelper.AddOrUpdateInList<GoalListItem, GoalDescription>(m_SerialisedGoals, m_ListItems[i]);
			}

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveGoals();

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
			bool shouldSaveToFile = PuppeteerEditorHelper.AddOrUpdateInList<GoalListItem, GoalDescription>(m_SerialisedGoals, m_SelectedListItem);

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveGoals();

			if (successful)
			{
				m_SelectedListItem.MarkUnsavedChanges(false);
				TryClearUnsavedMarker();
			}
		}

		public override bool TryOpenEntry(Guid _guid)
		{
			var matchingItem = m_ListItems.Find(_entry => (_entry as GoalListItem).GetDescription().GUID == _guid);

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
			if (m_GoalConfigurator != null)
			{
				return;
			}

			m_GoalConfigurator = new GoalConfigurator
			{
				AddButton = m_ConfiguratorElement.Q<Button>(name: "addButton"),
				DisplayName = m_ConfiguratorElement.Q<TextField>(name: "displayName"),
				GoalPartsContainer = m_ConfiguratorElement.Q<VisualElement>(name: "goalParts"),
				BaseUtility = m_ConfiguratorElement.Q<FloatField>(name: "baseUtility"),
				UtilityContainer = m_ConfiguratorElement.Q<VisualElement>(name: "utilityParts"),
				AddUtilityButton = m_ConfiguratorElement.Q<Button>(name: "addUtilityButton"),
				GUIDLabel = m_RightPanelContent.Q<Label>(name: "GUIDLabel"),
			};

			m_GoalConfigurator.AddButton.clickable.clicked += AddNewGoalPartItem;
			m_GoalConfigurator.AddUtilityButton.clickable.clicked += AddNewUtilityItem;

			RegisterConfiguratorCallbacks();
		}

		protected override void UpdateConfigurator()
		{
			if (m_SelectedListItem is GoalListItem selectedGoalListItem)
			{
				EnableRightPanelContent();
				LazyInitConfigurator();

				GoalDescription selectedDescription = selectedGoalListItem.GetDescription();

				m_GoalConfigurator.DisplayName.value = selectedDescription.DisplayName;
				m_GoalConfigurator.GoalPartsContainer.Clear();
				m_GoalConfigurator.BaseUtility.value = selectedDescription.BaseUtility;
				m_GoalConfigurator.UtilityContainer.Clear();
				m_GoalConfigurator.GUIDLabel.text = selectedDescription.GUID.ToString();

				m_SelectedGoalPartItem = null;
				m_SelectedWorkingMemoryUtilityItem = null;

				for (int i = 0; i < selectedDescription.GoalParts.Length; ++i)
				{
					CreateGoalPartItem(selectedDescription.GoalParts[i]);
				}
				if (selectedDescription.UtilityParts != null)
				{
					for (int i = 0; i < selectedDescription.UtilityParts.Length; ++i)
					{
						CreateUtilityItem(selectedDescription.UtilityParts[i]);
					}
				}
			}
		}

		private void AddNewGoalListItem()
		{
			GoalDescription goalDescription = new GoalDescription
			{
				DisplayName = "New Goal",
				GoalParts = new WorldStateDescription[] { new WorldStateDescription() { Key = "New Goal Part", Value = false } },
				UtilityParts = new UtilityDescription[] { },
				BaseUtility = 5,
			};

			AddNewGoalListItem(goalDescription);
		}

		private void AddNewGoalListItem(GoalDescription _goalDescription)
		{
			GoalDescription newGoalDescription = new GoalDescription
			{
				GUID = Guid.NewGuid(),
				DisplayName = _goalDescription.DisplayName,
				GoalParts = new WorldStateDescription[_goalDescription.GoalParts.Length],
				UtilityParts = new UtilityDescription[_goalDescription.UtilityParts.Length],
				BaseUtility = _goalDescription.BaseUtility,
			};

			for (int i = 0; i < _goalDescription.GoalParts.Length; ++i)
			{
				newGoalDescription.GoalParts[i] = new WorldStateDescription(_goalDescription.GoalParts[i]);
			}

			for (int i = 0; i < _goalDescription.UtilityParts.Length; ++i)
			{
				newGoalDescription.UtilityParts[i] = new UtilityDescription(_goalDescription.UtilityParts[i]);
			}

			GoalListItem item = CreateGoalListItem(newGoalDescription);
			m_ListItemScrollView.Add(item);

			item.MarkUnsavedChanges(true);
			AddUnsavedMarker();

			UpdateSelectedListItem(item);
		}

		private void AddNewGoalPartItem()
		{
			AddNewGoalPartItem(new WorldStateDescription() { Key = "New Goal Part", Value = false });
		}

		private void AddNewGoalPartItem(WorldStateDescription _goalPart)
		{
			if (m_SelectedListItem is GoalListItem selectedGoalListItem)
			{
				GoalDescription selectedDescription = selectedGoalListItem.GetDescription();

				int newIndex = PuppeteerEditorHelper.Append(ref selectedDescription.GoalParts, _goalPart);

				WorldStateItem goalPartItem = CreateGoalPartItem(selectedDescription.GoalParts[newIndex]);

				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();

				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, goalPartItem);
			}
		}

		private void AddNewUtilityItem()
		{
			AddNewUtilityItem(new UtilityDescription()
			{
				WorldStateName = "New Utility Curve",
				UtilityCurve = AnimationCurve.Linear(0, 0, 1, 1),
				CurveMultiplier = 1
			});
		}

		private void AddNewUtilityItem(UtilityDescription _utilityDescription)
		{
			if (m_SelectedListItem is GoalListItem selectedGoalListItem)
			{
				GoalDescription selectedDescription = selectedGoalListItem.GetDescription();

				int newIndex = PuppeteerEditorHelper.Append(ref selectedDescription.UtilityParts, _utilityDescription);

				WorkingMemoryUtilityItem utilityItem = CreateUtilityItem(selectedDescription.UtilityParts[newIndex]);

				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();

				UpdateSelectedUtilityItem(utilityItem);
			}
		}

		private void AnyUtilityItemValueChanged()
		{
			m_SelectedListItem.MarkUnsavedChanges(true);
			AddUnsavedMarker();
		}

		private GoalListItem CreateGoalListItem(GoalDescription _goalDescription)
		{
			GoalListItem item = new GoalListItem(_goalDescription);
			item.OnMouseDown += UpdateSelectedListItem;
			item.OnDelete += DeleteListItem;
			item.OnDuplicate += _item => AddNewGoalListItem((_item as GoalListItem).GetDescription());
			m_ListItems.Add(item);

			return item;
		}

		private WorldStateItem CreateGoalPartItem(WorldStateDescription _goalpart)
		{
			WorldStateItem goalPartItem = new WorldStateItem(_goalpart);
			goalPartItem.OnMouseDown += _item =>
			{
				if (m_SelectedWorkingMemoryUtilityItem != null)
				{
					UpdateSelectedUtilityItem(null);
				}

				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, _item);
			};
			goalPartItem.OnDelete += _item =>
			{
				PuppeteerEditorHelper.DeleteWorldStateItem(this, m_SelectedListItem as GoalListItem,
					ref (m_SelectedListItem as GoalListItem).GetDescription().GoalParts, _item, SelectNeighbourGoalPartItemIfNeeded);
			};

			goalPartItem.OnDuplicate += _item => AddNewGoalPartItem(new WorldStateDescription(_item.GetWorldStateDescription()));
			goalPartItem.OnValueChanged += _item =>
			{
				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();
			};

			m_GoalConfigurator.GoalPartsContainer.Add(goalPartItem);

			return goalPartItem;
		}

		private WorkingMemoryUtilityItem CreateUtilityItem(UtilityDescription _utilityDescription)
		{
			WorkingMemoryUtilityItem utilityItem = new WorkingMemoryUtilityItem(
				_utilityDescription,
				AnyUtilityItemValueChanged,
				_item =>
				{
					if (m_SelectedGoalPartItem != null)
					{
						PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, null);
					}

					UpdateSelectedUtilityItem(_item);
				},
				DeleteUtilityItem,
				DuplicateUtilityItem
				);

			m_GoalConfigurator.UtilityContainer.Add(utilityItem);
			return utilityItem;
		}

		private void DeleteUtilityItem(WorkingMemoryUtilityItem _utilityItem)
		{
			SelectNeighbourUtilityItemIfNeeded(_utilityItem);
			_utilityItem.RemoveFromHierarchy();

			PuppeteerEditorHelper.Remove(ref (m_SelectedListItem as GoalListItem).GetDescription().UtilityParts, _utilityItem.GetUtilityDescription());

			m_SelectedListItem.MarkUnsavedChanges(true);
			AddUnsavedMarker();
		}

		private void DuplicateUtilityItem(WorkingMemoryUtilityItem _utilityItem)
		{
			AddNewUtilityItem(new UtilityDescription(_utilityItem.GetUtilityDescription()));
		}

		private void ListItemSelectedOrRemoved(ListItem _listItem)
		{
			if (_listItem == null)
			{
				m_OnUpdateSerialisedLastSelected?.Invoke(Guid.Empty);
			}
			else
			{
				m_OnUpdateSerialisedLastSelected?.Invoke((_listItem as GoalListItem).GetDescription().GUID);
			}
		}

		private void RegisterConfiguratorCallbacks()
		{
			m_GoalConfigurator.DisplayName.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary((_eventTarget.target as TextField), ref (m_SelectedListItem as GoalListItem).GetDescription().DisplayName))
				{
					m_SelectedListItem.ChangeText((_eventTarget.target as TextField).value);
					m_SelectedListItem.MarkUnsavedChanges(true);
					AddUnsavedMarker();
				}
			});

			m_GoalConfigurator.BaseUtility.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (m_SelectedListItem is GoalListItem selectedGoalListItem)
				{
					float currentUtility = selectedGoalListItem.GetDescription().BaseUtility;
					float newUtility = (_eventTarget.target as FloatField).value;

					if (currentUtility != newUtility)
					{
						selectedGoalListItem.GetDescription().BaseUtility = newUtility;
						selectedGoalListItem.MarkUnsavedChanges(true);
						AddUnsavedMarker();
					}
				}
			});
		}

		private bool SelectNeighbourGoalPartItem(MoveDirection _direction)
		{
			if (_direction != MoveDirection.Up && _direction != MoveDirection.Down)
			{
				return false;
			}

			if (m_SelectedListItem is GoalListItem selectedGoalListItem)
			{
				GoalDescription description = selectedGoalListItem.GetDescription();
				int index = m_GoalConfigurator.GoalPartsContainer.IndexOf(m_SelectedGoalPartItem);
				if (index > 0 && _direction == MoveDirection.Up)
				{
					PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, m_GoalConfigurator.GoalPartsContainer[--index] as WorldStateItem);
					return true;
				}
				else if (_direction == MoveDirection.Down)
				{
					if (index < description.GoalParts.Length - 1)
					{
						PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, m_GoalConfigurator.GoalPartsContainer[++index] as WorldStateItem);
					}
					else
					{
						if (m_GoalConfigurator.UtilityContainer.childCount > 0)
						{
							PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, null);
							UpdateSelectedUtilityItem(m_GoalConfigurator.UtilityContainer[0] as WorkingMemoryUtilityItem);
						}
					}
					return true;
				}
			}
			return false;
		}

		private void SelectNeighbourGoalPartItemIfNeeded(WorldStateItem _goalPartItem)
		{
			if (_goalPartItem != m_SelectedGoalPartItem)
			{
				return;
			}

			bool neighbourSelected = SelectNeighbourGoalPartItem(MoveDirection.Up);
			if (!neighbourSelected)
			{
				neighbourSelected |= SelectNeighbourGoalPartItem(MoveDirection.Down);
			}

			if (!neighbourSelected)
			{
				m_SelectedGoalPartItem = null;
			}
		}

		private bool SelectNeighbourUtilityItem(MoveDirection _direction)
		{
			if (_direction != MoveDirection.Up && _direction != MoveDirection.Down)
			{
				return false;
			}

			if (m_SelectedListItem is GoalListItem selectedGoalListItem)
			{
				GoalDescription description = selectedGoalListItem.GetDescription();
				int index = m_GoalConfigurator.UtilityContainer.IndexOf(m_SelectedWorkingMemoryUtilityItem);
				if (_direction == MoveDirection.Up)
				{
					if (index > 0)
					{
						UpdateSelectedUtilityItem(m_GoalConfigurator.UtilityContainer[--index] as WorkingMemoryUtilityItem);
					}
					else if (index == 0)
					{
						int goalPartsChildCount = m_GoalConfigurator.GoalPartsContainer.childCount;
						if (goalPartsChildCount > 0)
						{
							PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedGoalPartItem, m_GoalConfigurator.GoalPartsContainer[--goalPartsChildCount] as WorldStateItem);
							UpdateSelectedUtilityItem(null);
						}
					}

					return true;
				}
				else if (_direction == MoveDirection.Down && index < description.UtilityParts.Length - 1)
				{
					UpdateSelectedUtilityItem(m_GoalConfigurator.UtilityContainer[++index] as WorkingMemoryUtilityItem);
					return true;
				}
			}
			return false;
		}

		private void SelectNeighbourUtilityItemIfNeeded(WorkingMemoryUtilityItem _utilityItem)
		{
			if (_utilityItem != m_SelectedWorkingMemoryUtilityItem)
			{
				return;
			}

			bool neighbourSelected = SelectNeighbourUtilityItem(MoveDirection.Up);
			if (!neighbourSelected)
			{
				neighbourSelected |= SelectNeighbourUtilityItem(MoveDirection.Down);
			}

			if (!neighbourSelected)
			{
				m_SelectedWorkingMemoryUtilityItem = null;
			}
		}

		private void UpdateSelectedUtilityItem(WorkingMemoryUtilityItem _utilityItem)
		{
			if (m_SelectedWorkingMemoryUtilityItem != null)
			{
				m_SelectedWorkingMemoryUtilityItem.RemoveFromClassList("selectedWorldStateItem");
				if (_utilityItem == m_SelectedWorkingMemoryUtilityItem)
				{
					m_SelectedWorkingMemoryUtilityItem = null;
					return;
				}
			}
			m_SelectedWorkingMemoryUtilityItem = _utilityItem;
			_utilityItem?.AddToClassList("selectedWorldStateItem");
		}

		private readonly ContextualMenuManipulator m_ContextMenuScrollView;
		private readonly Action<Guid> m_OnUpdateSerialisedLastSelected;
		private readonly List<GoalDescription> m_SerialisedGoals;
		private GoalConfigurator m_GoalConfigurator = null;
		private WorldStateItem m_SelectedGoalPartItem = null;
		private WorkingMemoryUtilityItem m_SelectedWorkingMemoryUtilityItem = null;

		private class GoalConfigurator
		{
			public Button AddButton;
			public Button AddUtilityButton;
			public FloatField BaseUtility;
			public TextField DisplayName;
			public VisualElement GoalPartsContainer;
			public Label GUIDLabel;
			public VisualElement UtilityContainer;
		}
	}
}