using System;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	public class ListItem : VisualElement
	{
		public void ChangeText(string _text)
		{
			m_Text = _text;
			m_TextElement.text = _text;
		}

		public bool IsMarkedForUnsavedChanges()
		{
			return m_UnsavedChanges;
		}

		public void MarkUnsavedChanges(bool _hasUnsavedChanges)
		{
			m_UnsavedChanges = _hasUnsavedChanges;
			m_TextElement.text = _hasUnsavedChanges ? m_Text + UNSAVED_CHANGES_POSTFIX : m_Text;
		}

		protected void Init()
		{
			AddToClassList(USS_CLASS_NAME);

			m_TextElement = new TextElement { name = "text", text = m_Text, pickingMode = PickingMode.Ignore };
			m_TextElement.AddToClassList(USS_CLASS_NAME_TEXT);
			hierarchy.Add(m_TextElement);
			this.AddManipulator(new Clickable(() => OnMouseDown?.Invoke(this)));
		}

		public static readonly string UNSAVED_CHANGES_POSTFIX = "*";
		public static readonly string USS_CLASS_NAME = "listItem";
		public static readonly string USS_CLASS_NAME_TEXT = USS_CLASS_NAME + "Text";

		public Action<ListItem> OnMouseDown;

		protected string m_Text;

		private TextElement m_TextElement;
		private bool m_UnsavedChanges = false;
	}
}