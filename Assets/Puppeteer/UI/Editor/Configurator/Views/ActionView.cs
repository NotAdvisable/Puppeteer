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
	internal class ActionView : PuppeteerView
	{
		public ActionView(VisualElement _rootElement, VisualElement _leftPanel, VisualElement _rightPanel, Action<Guid> _updateLastSelectedCallback)
			: base(_rootElement, _leftPanel, _rightPanel)
		{
			m_Label = "Actions";
			tooltip = "Create and configure the actions that agents use to achieve their goals.";

			var visualTreeAsset = Resources.Load<VisualTreeAsset>("LayoutActionConfigurator");
			visualTreeAsset.CloneTree(m_ConfiguratorElement);

			PuppeteerManager.Instance.LoadDescriptionsOfType<ActionDescription>();

			m_SerialisedActions = PuppeteerManager.Instance.GetActionDescriptions();

			for (int i = 0; i < m_SerialisedActions.Count; ++i)
			{
				CreateActionListItem(m_SerialisedActions[i]);
			}

			m_ContextMenuScrollView = new ContextualMenuManipulator(_menubuilder =>
			{
				_menubuilder.menu.AppendAction("Create New Action", _dropDownMenuAction => { AddNewActionListItem(); }, DropdownMenuAction.Status.Normal);
			});

			m_OnUpdateSerialisedLastSelected = _updateLastSelectedCallback;
			OnListItemSelectedOrRemoved = ListItemSelectedOrRemoved;
		}

		public override void AddNewBasedOnSelection()
		{
			if (m_SelectedWorldStateItem != null)
			{
				if (m_ActionConfigurator == null)
				{
					return;
				}

				if (m_ActionConfigurator.EffectsContainer.Contains(m_SelectedWorldStateItem))
				{
					AddNewWorldStateItem(m_ActionConfigurator.EffectsContainer, "New Effect");
				}
				else
				{
					AddNewWorldStateItem(m_ActionConfigurator.PreconditionsContainer, "New Precondition");
				}
			}
			else if (m_SelectedListItem != null)
			{
				AddNewActionListItem();
			}
		}

		public override void ClearSelection()
		{
			if (m_SelectedWorldStateItem != null)
			{
				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedWorldStateItem, null);
			}
			else
			{
				base.ClearSelection();
			}
		}

		public override void CloseView()
		{
			base.CloseView();

			m_AddButton.clickable.clicked -= AddNewActionListItem;

			m_ListItemScrollView.RemoveManipulator(m_ContextMenuScrollView);
		}

		public override void DeleteSelection()
		{
			if (m_SelectedWorldStateItem != null && m_SelectedListItem is ActionListItem selectedActionListItem)
			{
				PuppeteerEditorHelper.DeleteWorldStateItem(
					_view: this,
					_selectedListItem: selectedActionListItem,
					_worldStateDescriptions: ref (m_ActionConfigurator.EffectsContainer.Contains(m_SelectedWorldStateItem)
													? ref selectedActionListItem.GetDescription().Effects
													: ref selectedActionListItem.GetDescription().Preconditions),
					_worldStateItem: m_SelectedWorldStateItem,
					_selectNeighbourFunc: null);
			}
			else
			{
				base.DeleteSelection();
			}
		}

		public override void DuplicateSelection()
		{
			if (m_SelectedWorldStateItem != null)
			{
				AddNewWorldStateItem(m_SelectedWorldStateItem.parent, new WorldStateDescription(m_SelectedWorldStateItem.GetWorldStateDescription()));
			}
			else if (m_SelectedListItem != null)
			{
				AddNewActionListItem((m_SelectedListItem as ActionListItem).GetDescription());
			}
		}

		public override void EnterSelection()
		{
			if (m_SelectedWorldStateItem != null)
			{
				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedWorldStateItem, null);
				return;
			}

			if (m_SelectedListItem == null || m_ActionConfigurator == null)
			{
				return;
			}

			if (m_ActionConfigurator.PreconditionsContainer.childCount > 0)
			{
				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedWorldStateItem, m_ActionConfigurator.PreconditionsContainer[0] as WorldStateItem);
				return;
			}

			if (m_ActionConfigurator.EffectsContainer.childCount > 0)
			{
				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedWorldStateItem, m_ActionConfigurator.EffectsContainer[0] as WorldStateItem);
				return;
			}
		}

		public override void MoveSelection(MoveDirection _direction)
		{
			base.MoveSelection(_direction);
		}

		public override void OpenView(SerialisedConfiguratorState _serialisedConfiguratorStates)
		{
			base.OpenView(_serialisedConfiguratorStates);

			m_ListItemScrollViewHeader.text = "Available Actions";
			m_ListItemScrollViewHeaderIcon.image = PuppeteerEditorResourceLoader.ActionIcon32.texture;

			m_AddButton.clickable.clicked += AddNewActionListItem;
			m_AddButton.tooltip = "Create a new action.";

			m_ListItemScrollView.AddManipulator(m_ContextMenuScrollView);

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				m_ListItemScrollView.Add(m_ListItems[i]);
			}

			if (m_SelectedListItem != null)
			{
				UpdateConfigurator();
			}
			else if (!_serialisedConfiguratorStates.LastOpenedAction.Equals(Guid.Empty))
			{
				TryOpenEntry(_serialisedConfiguratorStates.LastOpenedAction.Value);
			}
			else
			{
				DisableRightPanelContent();
			}
		}

		public override void SaveAllChanges()
		{
			bool shouldSaveToFile = PuppeteerEditorHelper.RemoveDeletedItems<ActionListItem, ActionDescription>(m_SerialisedActions, m_ListItems) > 0;

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				shouldSaveToFile |= PuppeteerEditorHelper.AddOrUpdateInList<ActionListItem, ActionDescription>(m_SerialisedActions, m_ListItems[i]);
			}

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveActions();

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
			bool shouldSaveToFile = PuppeteerEditorHelper.AddOrUpdateInList<ActionListItem, ActionDescription>(m_SerialisedActions, m_SelectedListItem);

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveActions();

			if (successful)
			{
				m_SelectedListItem.MarkUnsavedChanges(false);
				TryClearUnsavedMarker();
			}
		}

		public override bool TryOpenEntry(Guid _guid)
		{
			var matchingItem = m_ListItems.Find(_entry => (_entry as ActionListItem).GetDescription().GUID == _guid);

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
			if (m_ActionConfigurator != null)
			{
				return;
			}

			m_ActionConfigurator = new ActionConfigurator
			{
				DisplayName = m_ConfiguratorElement.Q<TextField>(name: "displayName"),
				GUIDLabel = m_RightPanelContent.Q<Label>(name: "GUIDLabel"),
				ActionField = m_ConfiguratorElement.Q<ExecutableActionField>(name: "executableAction"),
				AddPreconditionButton = m_ConfiguratorElement.Q<Button>(name: "addPreconditionButton"),
				PreconditionsContainer = m_ConfiguratorElement.Q<VisualElement>(name: "preconditions"),
				AddEffectButton = m_ConfiguratorElement.Q<Button>(name: "addEffectButton"),
				EffectsContainer = m_ConfiguratorElement.Q<VisualElement>(name: "effects"),
				BaseUtility = m_ConfiguratorElement.Q<FloatField>(name: "baseUtility"),
			};

			m_ActionConfigurator.AddEffectButton.clickable.clicked += () =>
			{
				AddNewWorldStateItem(m_ActionConfigurator.EffectsContainer, "New Effect");
			};

			m_ActionConfigurator.AddPreconditionButton.clickable.clicked += () =>
			{
				AddNewWorldStateItem(m_ActionConfigurator.PreconditionsContainer, "New Precondition");
			};

			RegisterConfiguratorCallbacks();
		}

		protected override void UpdateConfigurator()
		{
			if (m_SelectedListItem is ActionListItem selectedActionListItem)
			{
				EnableRightPanelContent();
				LazyInitConfigurator();

				ActionDescription selectedDescription = selectedActionListItem.GetDescription();

				m_ActionConfigurator.DisplayName.value = selectedDescription.DisplayName;
				m_ActionConfigurator.GUIDLabel.text = selectedDescription.GUID.ToString();
				m_ActionConfigurator.BaseUtility.value = selectedDescription.BaseUtility;

				if (selectedDescription.ExecutableActionType != null)
				{
					m_ActionConfigurator.ActionField.SetValueWithoutNotify(selectedDescription.ExecutableActionType);
				}

				m_ActionConfigurator.PreconditionsContainer.Clear();
				for (int i = 0; i < selectedDescription.Preconditions.Length; ++i)
				{
					WorldStateItem worldStateItem = CreateWorldStateItem(selectedDescription.Preconditions[i]);
					worldStateItem.OnDelete += _item =>
					{
						PuppeteerEditorHelper.DeleteWorldStateItem(this, m_SelectedListItem as ActionListItem,
							ref (m_SelectedListItem as ActionListItem).GetDescription().Preconditions, _item, null);
					};

					m_ActionConfigurator.PreconditionsContainer.Add(worldStateItem);
				}

				m_ActionConfigurator.EffectsContainer.Clear();
				for (int i = 0; i < selectedDescription.Effects.Length; ++i)
				{
					WorldStateItem worldStateItem = CreateWorldStateItem(selectedDescription.Effects[i]);
					worldStateItem.OnDelete += _item =>
					{
						PuppeteerEditorHelper.DeleteWorldStateItem(this, m_SelectedListItem as ActionListItem,
							ref (m_SelectedListItem as ActionListItem).GetDescription().Effects, _item, null);
					};

					m_ActionConfigurator.EffectsContainer.Add(worldStateItem);
				}
			}
		}

		private void AddNewActionListItem()
		{
			ActionDescription actionDescription = new ActionDescription
			{
				DisplayName = "New Action",
				ExecutableActionType = typeof(DefaultAction),
				Preconditions = new WorldStateDescription[0],
				Effects = new WorldStateDescription[0],
				BaseUtility = 1,
			};

			AddNewActionListItem(actionDescription);
		}

		private void AddNewActionListItem(ActionDescription _actionDescription)
		{
			ActionDescription newActionDescription = new ActionDescription
			{
				GUID = System.Guid.NewGuid(),
				DisplayName = _actionDescription.DisplayName,
				ExecutableActionType = _actionDescription.ExecutableActionType,
				Preconditions = new WorldStateDescription[_actionDescription.Preconditions.Length],
				Effects = new WorldStateDescription[_actionDescription.Effects.Length],
				BaseUtility = _actionDescription.BaseUtility,
			};

			for (int i = 0; i < _actionDescription.Preconditions.Length; ++i)
			{
				newActionDescription.Preconditions[i] = new WorldStateDescription(_actionDescription.Preconditions[i]);
			}
			
			for (int i = 0; i < _actionDescription.Effects.Length; ++i)
			{
				newActionDescription.Effects[i] = new WorldStateDescription(_actionDescription.Effects[i]);
			}

			ActionListItem item = CreateActionListItem(newActionDescription);
			m_ListItemScrollView.Add(item);

			item.MarkUnsavedChanges(true);
			AddUnsavedMarker();

			UpdateSelectedListItem(item);
		}

		private void AddNewWorldStateItem(VisualElement _targetContainer, string _key)
		{
			AddNewWorldStateItem(_targetContainer, new WorldStateDescription() { Key = _key, Value = false });
		}

		private void AddNewWorldStateItem(VisualElement _targetContainer, WorldStateDescription _worldStateDescription)
		{
			if (m_SelectedListItem is ActionListItem selectedActionListItem)
			{
				ActionDescription selectedDescription = selectedActionListItem.GetDescription();

				WorldStateItem worldStateItem;

				if (_targetContainer == m_ActionConfigurator.EffectsContainer)
				{
					int newIndex = PuppeteerEditorHelper.Append(ref selectedDescription.Effects, _worldStateDescription);
					worldStateItem = CreateWorldStateItem(selectedDescription.Effects[newIndex]);
					worldStateItem.OnDelete += _item =>
					{
						PuppeteerEditorHelper.DeleteWorldStateItem(this, m_SelectedListItem as ActionListItem,
							ref (m_SelectedListItem as ActionListItem).GetDescription().Effects, _item, null);
					};
				}
				else
				{
					int newIndex = PuppeteerEditorHelper.Append(ref selectedDescription.Preconditions, _worldStateDescription);
					worldStateItem = CreateWorldStateItem(selectedDescription.Preconditions[newIndex]);
					worldStateItem.OnDelete += _item =>
					{
						PuppeteerEditorHelper.DeleteWorldStateItem(this, m_SelectedListItem as ActionListItem,
							ref (m_SelectedListItem as ActionListItem).GetDescription().Preconditions, _item, null);
					};
				}

				_targetContainer.Add(worldStateItem);

				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();

				PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedWorldStateItem, worldStateItem);
			}
		}

		private ActionListItem CreateActionListItem(ActionDescription _description)
		{
			ValidateAndFixDescription(ref _description);
			ActionListItem item = new ActionListItem(_description);
			item.OnMouseDown += UpdateSelectedListItem;
			item.OnDelete += DeleteListItem;
			item.OnDuplicate += _item => AddNewActionListItem((_item as ActionListItem).GetDescription());
			m_ListItems.Add(item);

			return item;
		}

		private WorldStateItem CreateWorldStateItem(WorldStateDescription _worldStateDescription)
		{
			WorldStateItem worldStateItem = new WorldStateItem(_worldStateDescription);
			worldStateItem.OnMouseDown += _item => PuppeteerEditorHelper.UpdateSelectedWorldStateItem(ref m_SelectedWorldStateItem, _item);
			worldStateItem.OnDuplicate += _item => AddNewWorldStateItem(_item.parent, new WorldStateDescription(_item.GetWorldStateDescription()));
			worldStateItem.OnValueChanged += _item =>
			{
				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();
			};

			return worldStateItem;
		}

		private void ListItemSelectedOrRemoved(ListItem _listItem)
		{
			if (_listItem == null)
			{
				m_OnUpdateSerialisedLastSelected?.Invoke(Guid.Empty);
			}
			else
			{
				m_OnUpdateSerialisedLastSelected?.Invoke((_listItem as ActionListItem).GetDescription().GUID);
			}
		}

		private void RegisterConfiguratorCallbacks()
		{
			m_ActionConfigurator.ActionField.RegisterCallback<ChangeEvent<Type>>(_event =>
			{
				(m_SelectedListItem as ActionListItem).GetDescription().ExecutableActionType = _event.newValue;
				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();
			});

			m_ActionConfigurator.DisplayName.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary((_eventTarget.target as TextField), ref (m_SelectedListItem as ActionListItem).GetDescription().DisplayName))
				{
					m_SelectedListItem.ChangeText((_eventTarget.target as TextField).value);
					m_SelectedListItem.MarkUnsavedChanges(true);
					AddUnsavedMarker();
				}
			});

			m_ActionConfigurator.BaseUtility.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (m_SelectedListItem is ActionListItem selectedActionListItem)
				{
					float currentUtility = selectedActionListItem.GetDescription().BaseUtility;
					float newUtility = (_eventTarget.target as FloatField).value;

					newUtility = Mathf.Max(newUtility, 1);

					if (currentUtility != newUtility)
					{
						selectedActionListItem.GetDescription().BaseUtility = newUtility;
						selectedActionListItem.MarkUnsavedChanges(true);
						AddUnsavedMarker();
					}
				}
			});
		}

		private void ValidateAndFixDescription(ref ActionDescription _description)
		{
			if (_description.Preconditions == null)
			{
				_description.Preconditions = new WorldStateDescription[0];
			}

			if (_description.Effects == null)
			{
				_description.Effects = new WorldStateDescription[0];
			}
		}

		private readonly ContextualMenuManipulator m_ContextMenuScrollView;
		private readonly Action<Guid> m_OnUpdateSerialisedLastSelected;
		private readonly List<ActionDescription> m_SerialisedActions;
		private ActionConfigurator m_ActionConfigurator = null;
		private WorldStateItem m_SelectedWorldStateItem = null;

		private class ActionConfigurator
		{
			public ExecutableActionField ActionField;
			public Button AddEffectButton;
			public Button AddPreconditionButton;
			public FloatField BaseUtility;
			public TextField DisplayName;
			public VisualElement EffectsContainer;
			public Label GUIDLabel;
			public VisualElement PreconditionsContainer;
		}
	}
}