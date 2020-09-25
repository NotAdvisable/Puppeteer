using Puppeteer.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("Puppeteer.UI", "pui")]

namespace Puppeteer.UI
{
	public class UtilityOperatorSelectorField : PopupField<UtilityOperators>
	{
		public UtilityOperatorSelectorField() : this(null)
		{
		}

		public UtilityOperatorSelectorField(string _label = null) : base(_label, GetValidUtilityOperators(), 0, GetUtilityOperatorName, GetUtilityOperatorName)
		{
			AddToClassList(ussClassName);
			labelElement.AddToClassList(labelUssClassName);
		}

		public static string GetUtilityOperatorName(UtilityOperators _operator)
		{
			return PuppeteerEditorHelper.UtilityOperatorToString(_operator);
		}

		private static List<UtilityOperators> GetValidUtilityOperators()
		{
			return new List<UtilityOperators>
			{
				UtilityOperators.PLUS_EQUALS,
				UtilityOperators.MINUS_EQUALS,
				UtilityOperators.MULTIPLY_EQUALS,
				UtilityOperators.DIVIDE_EQUALS,
				UtilityOperators.MODULO_EQUALS,
			};
		}

		public new class UxmlFactory : UxmlFactory<UtilityOperatorSelectorField, UxmlTraits>
		{
		}

		public new class UxmlTraits : PopupField<Guid>.UxmlTraits
		{
			public override void Init(VisualElement _ve, IUxmlAttributes _bag, CreationContext _cc)
			{
				base.Init(_ve, _bag, _cc);

				UtilityOperatorSelectorField utilityOperatorSelectorField = (UtilityOperatorSelectorField)_ve;

				string valueFromBag = m_Value.GetValueFromBag(_bag, _cc);

				KeyValuePair<UtilityOperators, string> validPair = PuppeteerEditorHelper.GetUtilityOperatorStringPairs().FirstOrDefault(_entry =>
				{
					return _entry.Value == valueFromBag;
				});

				if (validPair.Value != string.Empty)
				{
					utilityOperatorSelectorField.SetValueWithoutNotify(validPair.Key);
				}
			}

			private readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value" };
		}
	}
}