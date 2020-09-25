using Puppeteer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("Puppeteer.UI", "pui")]

namespace Puppeteer.UI
{
	public class ArchetypeSelectorField : PopupField<Guid>
	{
		public ArchetypeSelectorField() : this(null)
		{
		}

		public ArchetypeSelectorField(string _label = null) : base(_label, PuppeteerEditorHelper.GetArchetypeSelectorGUIDs(), 0, GetArchetypeName, GetArchetypeName)
		{
			// Unity won't allow access to the choices member variable in BasePopupField so we unfortunately have to get it again.
			m_ArchetypeGuids = new List<Guid>(PuppeteerEditorHelper.GetArchetypeSelectorGUIDs());

			AddToClassList(ussClassName);
			labelElement.AddToClassList(labelUssClassName);
		}

		public bool IsInSync()
		{
			return Enumerable.SequenceEqual(m_ArchetypeGuids, PuppeteerEditorHelper.GetArchetypeSelectorGUIDs());
		}

		public static string GetArchetypeName(Guid _guid)
		{
			if (_guid == Guid.Empty)
			{
				return EMPTY_GUID_TEXT;
			}

			return PuppeteerManager.Instance.GetArchetypeDescription(_guid)?.DisplayName;
		}

		public List<Guid> GetArchetypeGUIDs()
		{
			return m_ArchetypeGuids;
		}

		private static readonly string EMPTY_GUID_TEXT = "-";
		private readonly List<Guid> m_ArchetypeGuids;

		public new class UxmlFactory : UxmlFactory<ArchetypeSelectorField, UxmlTraits>
		{
		}

		public new class UxmlTraits : PopupField<Guid>.UxmlTraits
		{
			public override void Init(VisualElement _ve, IUxmlAttributes _bag, CreationContext _cc)
			{
				base.Init(_ve, _bag, _cc);

				ArchetypeSelectorField archetypeSelectorField = (ArchetypeSelectorField)_ve;

				string valueFromBag = m_Value.GetValueFromBag(_bag, _cc);

				Guid validGuid = archetypeSelectorField.GetArchetypeGUIDs().FirstOrDefault(_entry =>
				{
				   return PuppeteerManager.Instance.GetArchetypeDescription(_entry)?.DisplayName == valueFromBag;
				});

				if (validGuid != null)
				{
					archetypeSelectorField.SetValueWithoutNotify(validGuid);
				}
			}

			private readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value" };
		}
	}
}