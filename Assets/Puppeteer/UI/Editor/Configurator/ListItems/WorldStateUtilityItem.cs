using Puppeteer.Core.Configuration;
using Puppeteer.Core.Utility;
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class WorldStateUtilityItem : VisualElement
	{
		public WorldStateUtilityItem(UtilityDescription _worldStateUtilityDescription, Action _anyValueChangedCallback,
			Action<WorldStateUtilityItem> _mouseDownCallback, Action<WorldStateUtilityItem> _deleteCallback, Action<WorldStateUtilityItem> _duplicateCallback)
		{
			m_UtilityDescription = _worldStateUtilityDescription;
			OnAnyValueChanged = _anyValueChangedCallback;
			OnMouseDown = _mouseDownCallback;
			OnDelete = _deleteCallback;
			OnDuplicate = _duplicateCallback;

			Init();
		}

		public UtilityDescription GetUtilityDescription()
		{
			return m_UtilityDescription;
		}

		protected void Init()
		{
			AddToClassList(USS_CLASS_NAME);
			AddToClassList("worldStateItem");
			AddToClassList("rounded");

			m_WorldStateField = new TextField("World State") { value = m_UtilityDescription.WorldStateName };
			m_WorldStateField.maxLength = 30;
			hierarchy.Add(m_WorldStateField);

			m_OperatorSelectorField = new UtilityOperatorSelectorField("Operator") { value = m_UtilityDescription.UtilityOperator };
			hierarchy.Add(m_OperatorSelectorField);

			m_CurveField = new CurveField("Curve") { value = m_UtilityDescription.UtilityCurve };
			hierarchy.Add(m_CurveField);

			m_CurveMultiplierField = new FloatField("Curve Multiplier") { value = m_UtilityDescription.CurveMultiplier };
			hierarchy.Add(m_CurveMultiplierField);

			m_WorldStateField.RegisterValueChangedCallback(WorldStateNameChanged);
			m_OperatorSelectorField.RegisterValueChangedCallback(UtilityOperatorChanged);
			m_CurveField.RegisterValueChangedCallback(CurveChanged);
			m_CurveMultiplierField.RegisterValueChangedCallback(CurveMultiplierChanged);

			this.AddManipulator(new Clickable(() => OnMouseDown?.Invoke(this)));

			this.AddManipulator(new ContextualMenuManipulator(_menubuilder =>
			{
				_menubuilder.menu.AppendAction("Delete", _dropDownMenuAction => OnDelete?.Invoke(this), DropdownMenuAction.Status.Normal);
				_menubuilder.menu.AppendAction("Duplicate", _dropDownMenuAction => OnDuplicate?.Invoke(this), DropdownMenuAction.Status.Normal);
			}));
		}

		private void CurveChanged(ChangeEvent<AnimationCurve> _event)
		{
			m_UtilityDescription.UtilityCurve = _event.newValue;

			OnAnyValueChanged?.Invoke();
		}

		private void CurveMultiplierChanged(ChangeEvent<float> _event)
		{
			m_UtilityDescription.CurveMultiplier = _event.newValue;

			OnAnyValueChanged?.Invoke();
		}

		private void UtilityOperatorChanged(ChangeEvent<UtilityOperators> _event)
		{
			m_UtilityDescription.UtilityOperator = _event.newValue;

			OnAnyValueChanged?.Invoke();
		}

		private void WorldStateNameChanged(ChangeEvent<string> _event)
		{
			m_UtilityDescription.WorldStateName = _event.newValue;

			OnAnyValueChanged?.Invoke();
		}

		public static readonly string USS_CLASS_NAME = "worldStateUtilityItem";

		public Action OnAnyValueChanged;
		public Action<WorldStateUtilityItem> OnDelete;
		public Action<WorldStateUtilityItem> OnDuplicate;
		public Action<WorldStateUtilityItem> OnMouseDown;
		private readonly UtilityDescription m_UtilityDescription;

		private CurveField m_CurveField;
		private FloatField m_CurveMultiplierField;
		private UtilityOperatorSelectorField m_OperatorSelectorField;
		private TextField m_WorldStateField;
	}
}