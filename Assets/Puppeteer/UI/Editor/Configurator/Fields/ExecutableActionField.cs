using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("Puppeteer.UI", "pui")]

namespace Puppeteer.UI
{
	public class ExecutableActionField : PopupField<System.Type>
	{
		public ExecutableActionField() : this(null)
		{
		}

		public ExecutableActionField(string _label = null) : base(label: _label, PuppeteerEditorHelper.GetActionTypes(), 0)
		{
			// Unity won't let us access the choices member variable in BasePopupField so we unfortunately have to get it again.
			m_ContentTypes = PuppeteerEditorHelper.GetActionTypes();

			AddToClassList(ussClassName);
			labelElement.AddToClassList(labelUssClassName);
		}

		public List<System.Type> GetContentTypes()
		{
			return m_ContentTypes;
		}

		private readonly List<System.Type> m_ContentTypes;

		public new class UxmlFactory : UxmlFactory<ExecutableActionField, UxmlTraits>
		{
		}

		public new class UxmlTraits : PopupField<System.Type>.UxmlTraits
		{
			public override void Init(VisualElement _ve, IUxmlAttributes _bag, CreationContext _cc)
			{
				base.Init(_ve, _bag, _cc);

				ExecutableActionField executableActionField = (ExecutableActionField)_ve;

				string valueFromBag = m_Value.GetValueFromBag(_bag, _cc);

				System.Type validType = executableActionField.GetContentTypes().FirstOrDefault(_entry => _entry.Name == valueFromBag);

				if (validType != null)
				{
					executableActionField.SetValueWithoutNotify(validType);
				}
			}

			private readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value" };
		}
	}
}