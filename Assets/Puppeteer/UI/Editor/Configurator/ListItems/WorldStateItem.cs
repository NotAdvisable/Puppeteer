using Puppeteer.Core.Configuration;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class WorldStateItem : VisualElement
	{
		public WorldStateItem(WorldStateDescription _worldStateDescription)
		{
			m_WorldStateDescription = _worldStateDescription;
			Init();
		}

		public WorldStateDescription GetWorldStateDescription()
		{
			return m_WorldStateDescription;
		}

		protected void Init()
		{
			AddToClassList(USS_CLASS_NAME);
			AddToClassList("rounded");

			m_KeyField = new TextField("World State") { value = m_WorldStateDescription.Key };
			m_KeyField.tooltip = "The identifier of the world state.";
			
			m_KeyField.maxLength = 30;
			hierarchy.Add(m_KeyField);

			m_KeyField.RegisterCallback<ChangeEvent<string>>(KeyFieldChanged);

			hierarchy.Add(new Label("Desired Value"));

			ConfiguratorValueType? enumType = ConfiguratorTypeConverter.GetTypeFromObject(m_WorldStateDescription.Value);
			m_EnumFieldValueType = new EnumField("Type", enumType != null ? enumType : ConfiguratorValueType.BOOL)
			{
				tooltip = "Data type of the world state."
			};
			m_EnumFieldValueType.AddToClassList("desiredValue");
			hierarchy.Add(m_EnumFieldValueType);

			m_EnumFieldValueType.RegisterCallback<ChangeEvent<Enum>>(EnumChanged);

			m_ValueField = ConfiguratorTypeConverter.CreateFieldBasedOnType(enumType.Value, m_WorldStateDescription.Value, "Value", ValueFieldChanged);
			m_ValueField.tooltip = "Value of the world state.";
			m_ValueField.AddToClassList("desiredValue");
			hierarchy.Add(m_ValueField);

			this.AddManipulator(new Clickable(() => OnMouseDown?.Invoke(this)));

			this.AddManipulator(new ContextualMenuManipulator(_menubuilder =>
			{
				_menubuilder.menu.AppendAction("Delete", _dropDownMenuAction => OnDelete?.Invoke(this), DropdownMenuAction.Status.Normal);
				_menubuilder.menu.AppendAction("Duplicate", _dropDownMenuAction => OnDuplicate?.Invoke(this), DropdownMenuAction.Status.Normal);
			}));
		}

		private void EnumChanged(ChangeEvent<Enum> _event)
		{
			m_ValueField.RemoveFromHierarchy();
			ConfiguratorValueType newValue = (ConfiguratorValueType)_event.newValue;
			m_ValueField = ConfiguratorTypeConverter.CreateFieldBasedOnType(newValue, m_WorldStateDescription.Value, "Value", ValueFieldChanged);
			m_ValueField.AddToClassList("desiredValue");
			hierarchy.Add(m_ValueField);
		}

		private void KeyFieldChanged(ChangeEvent<string> _event)
		{
			m_WorldStateDescription.Key = _event.newValue;

			OnValueChanged?.Invoke(this);
		}

		private void ValueFieldChanged()
		{
			m_WorldStateDescription.Value = ConfiguratorTypeConverter.GetValueFromField(m_ValueField);

			OnValueChanged?.Invoke(this);
		}

		public static readonly string USS_CLASS_NAME = "worldStateItem";
		public static readonly string USS_CLASS_NAME_TEXT = USS_CLASS_NAME + "Text";

		public Action<WorldStateItem> OnDelete;
		public Action<WorldStateItem> OnDuplicate;
		public Action<WorldStateItem> OnMouseDown;
		public Action<WorldStateItem> OnValueChanged;

		private readonly WorldStateDescription m_WorldStateDescription;
		private EnumField m_EnumFieldValueType;
		private TextField m_KeyField;
		private VisualElement m_ValueField;
	}
}