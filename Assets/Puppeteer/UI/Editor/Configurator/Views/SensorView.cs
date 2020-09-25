using Puppeteer.Core;
using Puppeteer.Core.Configuration;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class SensorView : PuppeteerView
	{
		public SensorView(VisualElement _rootElement, VisualElement _leftPanel, VisualElement _rightPanel, Action<Guid> _updateLastSelectedCallback)
			: base(_rootElement, _leftPanel, _rightPanel)
		{
			m_Label = "Sensors";
			tooltip = "Create and configure the sensors that agents use to observe their environment.";

			var visualTreeAsset = Resources.Load<VisualTreeAsset>("LayoutSensorConfigurator");
			visualTreeAsset.CloneTree(m_ConfiguratorElement);

			PuppeteerManager.Instance.LoadDescriptionsOfType<SensorDescription>();

			m_SerialisedSensors = PuppeteerManager.Instance.GetSensorDescriptions();

			for (int i = 0; i < m_SerialisedSensors.Count; ++i)
			{
				CreateSensorListItem(m_SerialisedSensors[i]);
			}

			m_ContextMenuScrollView = new ContextualMenuManipulator(_menubuilder =>
			{
				_menubuilder.menu.AppendAction("Create New Sensor", _dropDownMenuAction => { AddNewSensorListItem(); }, DropdownMenuAction.Status.Normal);
			});

			m_OnUpdateSerialisedLastSelected = _updateLastSelectedCallback;
			OnListItemSelectedOrRemoved = ListItemSelectedOrRemoved;
		}

		public override void AddNewBasedOnSelection()
		{
			if (m_SelectedListItem != null)
			{
				AddNewSensorListItem();
			}
		}

		public override void CloseView()
		{
			base.CloseView();

			m_AddButton.clickable.clicked -= AddNewSensorListItem;

			m_ListItemScrollView.RemoveManipulator(m_ContextMenuScrollView);
		}

		public override void DuplicateSelection()
		{
			if (m_SelectedListItem != null)
			{
				AddNewSensorListItem((m_SelectedListItem as SensorListItem).GetDescription());
			}
		}

		public override void OpenView(SerialisedConfiguratorState _serialisedConfiguratorStates)
		{
			base.OpenView(_serialisedConfiguratorStates);

			m_ListItemScrollViewHeader.text = "Available Sensors";
			m_ListItemScrollViewHeaderIcon.image = PuppeteerEditorResourceLoader.SensorIcon32.texture;

			m_AddButton.clickable.clicked += AddNewSensorListItem;
			m_AddButton.tooltip = "Create a new sensor.";

			m_ListItemScrollView.AddManipulator(m_ContextMenuScrollView);

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				m_ListItemScrollView.Add(m_ListItems[i]);
			}

			if (m_SelectedListItem != null)
			{
				UpdateConfigurator();
			}
			else if (!_serialisedConfiguratorStates.LastOpenedSensor.Equals(Guid.Empty))
			{
				TryOpenEntry(_serialisedConfiguratorStates.LastOpenedSensor.Value);
			}
			else
			{
				DisableRightPanelContent();
			}
		}

		public override void SaveAllChanges()
		{
			bool shouldSaveToFile = PuppeteerEditorHelper.RemoveDeletedItems<SensorListItem, SensorDescription>(m_SerialisedSensors, m_ListItems) > 0;

			for (int i = 0; i < m_ListItems.Count; ++i)
			{
				shouldSaveToFile |= PuppeteerEditorHelper.AddOrUpdateInList<SensorListItem, SensorDescription>(m_SerialisedSensors, m_ListItems[i]);
			}

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveSensors();

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
			bool shouldSaveToFile = PuppeteerEditorHelper.AddOrUpdateInList<SensorListItem, SensorDescription>(m_SerialisedSensors, m_SelectedListItem);

			bool successful = !shouldSaveToFile || PuppeteerManager.Instance.SaveSensors();

			if (successful)
			{
				m_SelectedListItem.MarkUnsavedChanges(false);
				TryClearUnsavedMarker();
			}
		}

		public override bool TryOpenEntry(Guid _guid)
		{
			var matchingItem = m_ListItems.Find(_entry => (_entry as SensorListItem).GetDescription().GUID == _guid);

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
			if (m_SensorConfigurator != null)
			{
				return;
			}

			m_SensorConfigurator = new SensorConfigurator
			{
				DisplayName = m_ConfiguratorElement.Q<TextField>(name: "displayName"),
				GUIDLabel = m_RightPanelContent.Q<Label>(name: "GUIDLabel"),
				SensorField = m_ConfiguratorElement.Q<ExecutableSensorField>(name: "executableSensor"),
				ManagedWorldState = m_ConfiguratorElement.Q<TextField>(name: "managedWorldState"),
				ExecutionOrderField = m_ConfiguratorElement.Q<IntegerField>(name: "executionOrder"),
				TickRateField = m_ConfiguratorElement.Q<IntegerField>(name: "tickRate"),
				ShouldBeTickedToggle = m_ConfiguratorElement.Q<Toggle>(name: "shouldBeTicked"),
			};

			RegisterConfiguratorCallbacks();
		}

		protected override void UpdateConfigurator()
		{
			if (m_SelectedListItem is SensorListItem selectedSensorListItem)
			{
				EnableRightPanelContent();
				LazyInitConfigurator();

				SensorDescription selectedDescription = selectedSensorListItem.GetDescription();

				m_SensorConfigurator.DisplayName.value = selectedDescription.DisplayName;
				m_SensorConfigurator.GUIDLabel.text = selectedDescription.GUID.ToString();

				if (selectedDescription.ExecutableSensorType != null)
				{
					m_SensorConfigurator.SensorField.SetValueWithoutNotify(selectedDescription.ExecutableSensorType);
				}

				m_SensorConfigurator.ManagedWorldState.SetValueWithoutNotify(selectedDescription.ManagedWorldState);
				m_SensorConfigurator.ExecutionOrderField.SetValueWithoutNotify(selectedDescription.ExecutionOrder);
				m_SensorConfigurator.TickRateField.SetValueWithoutNotify(selectedDescription.TickRate);
				m_SensorConfigurator.ShouldBeTickedToggle.SetValueWithoutNotify(selectedDescription.ShouldBeTicked);

				m_SensorConfigurator.ExecutionOrderField.SetEnabled(selectedDescription.ShouldBeTicked);
				m_SensorConfigurator.TickRateField.SetEnabled(selectedDescription.ShouldBeTicked);
			}
		}

		private void AddNewSensorListItem()
		{
			SensorDescription sensorDescription = new SensorDescription
			{
				DisplayName = "New Sensor",
				ExecutableSensorType = typeof(DefaultSensor),
				ShouldBeTicked = true,
				ExecutionOrder = 0,
				TickRate = 50,
			};

			AddNewSensorListItem(sensorDescription);
		}

		private void AddNewSensorListItem(SensorDescription _sensorDescription)
		{
			SensorDescription newSensorDescription = new SensorDescription
			{
				GUID = System.Guid.NewGuid(),
				DisplayName = _sensorDescription.DisplayName,
				ExecutableSensorType = _sensorDescription.ExecutableSensorType,
				ShouldBeTicked = _sensorDescription.ShouldBeTicked,
				ExecutionOrder = _sensorDescription.ExecutionOrder,
				TickRate = _sensorDescription.TickRate,
			};

			SensorListItem item = CreateSensorListItem(newSensorDescription);
			m_ListItemScrollView.Add(item);

			item.MarkUnsavedChanges(true);
			AddUnsavedMarker();

			UpdateSelectedListItem(item);
		}

		private SensorListItem CreateSensorListItem(SensorDescription _description)
		{
			SensorListItem item = new SensorListItem(_description);
			item.OnMouseDown += UpdateSelectedListItem;
			item.OnDelete += DeleteListItem;
			item.OnDuplicate += _item => AddNewSensorListItem((_item as SensorListItem).GetDescription());
			m_ListItems.Add(item);

			return item;
		}

		private void ListItemSelectedOrRemoved(ListItem _listItem)
		{
			if (_listItem == null)
			{
				m_OnUpdateSerialisedLastSelected?.Invoke(Guid.Empty);
			}
			else
			{
				m_OnUpdateSerialisedLastSelected?.Invoke((_listItem as SensorListItem).GetDescription().GUID);
			}
		}

		private void RegisterConfiguratorCallbacks()
		{
			m_SensorConfigurator.SensorField.RegisterCallback<ChangeEvent<Type>>(_event =>
			{
				(m_SelectedListItem as SensorListItem).GetDescription().ExecutableSensorType = _event.newValue;
				m_SelectedListItem.MarkUnsavedChanges(true);
				AddUnsavedMarker();
			});

			m_SensorConfigurator.DisplayName.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary((_eventTarget.target as TextField), ref (m_SelectedListItem as SensorListItem).GetDescription().DisplayName))
				{
					m_SelectedListItem.ChangeText((_eventTarget.target as TextField).value);
					m_SelectedListItem.MarkUnsavedChanges(true);
					AddUnsavedMarker();
				}
			});

			m_SensorConfigurator.ManagedWorldState.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (m_SelectedListItem is SensorListItem selectedSensorListItem)
				{
					if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary((_eventTarget.target as TextField), ref selectedSensorListItem.GetDescription().ManagedWorldState))
					{
						selectedSensorListItem.MarkUnsavedChanges(true);
						AddUnsavedMarker();
					}
				}
			});

			m_SensorConfigurator.ExecutionOrderField.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (m_SelectedListItem is SensorListItem selectedSensorListItem)
				{
					if (_eventTarget.target is IntegerField targetField)
					{
						targetField.value = Mathf.Max(targetField.value, 0);
						if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary(targetField, ref selectedSensorListItem.GetDescription().ExecutionOrder))
						{
							selectedSensorListItem.MarkUnsavedChanges(true);
							AddUnsavedMarker();
						}
					}
				}
			});

			m_SensorConfigurator.TickRateField.RegisterCallback<FocusOutEvent>(_eventTarget =>
			{
				if (m_SelectedListItem is SensorListItem selectedSensorListItem)
				{
					if (_eventTarget.target is IntegerField targetField)
					{
						targetField.value = Mathf.Max(targetField.value, 1);
						if (PuppeteerEditorHelper.UpdateDescriptionIfNecessary(targetField, ref selectedSensorListItem.GetDescription().TickRate))
						{
							selectedSensorListItem.MarkUnsavedChanges(true);
							AddUnsavedMarker();
						}
					}
				}
			});

			m_SensorConfigurator.ShouldBeTickedToggle.RegisterValueChangedCallback(_changeEvent =>
			{
				if (m_SelectedListItem is SensorListItem selectedSensorListItem)
				{
					m_SensorConfigurator.TickRateField.SetEnabled(_changeEvent.newValue);
					m_SensorConfigurator.ExecutionOrderField.SetEnabled(_changeEvent.newValue);
					selectedSensorListItem.MarkUnsavedChanges(true);
					AddUnsavedMarker();
				}
			});
		}

		private readonly ContextualMenuManipulator m_ContextMenuScrollView;
		private readonly List<SensorDescription> m_SerialisedSensors;

		private readonly Action<Guid> m_OnUpdateSerialisedLastSelected;
		private SensorConfigurator m_SensorConfigurator = null;

		private class SensorConfigurator
		{
			public TextField DisplayName;
			public IntegerField ExecutionOrderField;
			public Label GUIDLabel;
			public TextField ManagedWorldState;
			public ExecutableSensorField SensorField;
			public Toggle ShouldBeTickedToggle;
			public IntegerField TickRateField;
		}
	}
}