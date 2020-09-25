using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("Puppeteer.UI", "pui")]

namespace Puppeteer.UI
{
	public class ExecutableSensorField : PopupField<System.Type>
	{
		public ExecutableSensorField() : this(null)
		{
		}

		public ExecutableSensorField(string _label = null) : base(label: _label, PuppeteerEditorHelper.GetSensorTypes(), 0)
		{
			// Unity won't let us access the choices member variable in BasePopupField so we unfortunately have to get it again.
			m_ContentTypes = PuppeteerEditorHelper.GetSensorTypes();

			AddToClassList(ussClassName);
			labelElement.AddToClassList(labelUssClassName);
		}

		public List<System.Type> GetContentTypes()
		{
			return m_ContentTypes;
		}

		private readonly List<System.Type> m_ContentTypes;

		public new class UxmlFactory : UxmlFactory<ExecutableSensorField, UxmlTraits>
		{
		}

		public new class UxmlTraits : PopupField<System.Type>.UxmlTraits
		{
			public override void Init(VisualElement _ve, IUxmlAttributes _bag, CreationContext _cc)
			{
				base.Init(_ve, _bag, _cc);

				ExecutableSensorField executableSensorField = (ExecutableSensorField)_ve;

				string valueFromBag = m_Value.GetValueFromBag(_bag, _cc);

				System.Type validType = executableSensorField.GetContentTypes().FirstOrDefault(_entry => _entry.Name == valueFromBag);

				if (validType != null)
				{
					executableSensorField.SetValueWithoutNotify(validType);
				}
			}

			private readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value" };
		}
	}
}