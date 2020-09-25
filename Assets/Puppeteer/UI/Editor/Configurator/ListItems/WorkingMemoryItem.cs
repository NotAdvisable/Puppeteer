using Puppeteer.Core.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class WorkingMemoryItem : VisualElement
	{
		public WorkingMemoryItem(string _key, object _value)
		{
			m_Key = _key;
			m_Value = _value;

			Init();
		}

		public WorkingMemoryItem(KeyValuePair<string, object> _keyValuePair)
		{
			m_Key = _keyValuePair.Key;
			m_Value = _keyValuePair.Value;

			AddToClassList("horizontalGroup");

			Init();
		}

		public string GetKey()
		{
			return m_Key;
		}

		public KeyValuePair<string, object> GetKeyValuePair()
		{
			return new KeyValuePair<string, object>(m_Key, m_Value);
		}

		public object GetValue()
		{
			return m_Value;
		}

		public void SetValueWithoutNotify(object _value)
		{
			m_Value = _value;
			m_ValueLabel.text = m_Value.ToString();
		}

		protected void Init()
		{
			AddToClassList(USS_CLASS_NAME);

			var keyLabel = new Label(m_Key);
			keyLabel.AddToClassList("workingMemoryKey");
			hierarchy.Add(keyLabel);

			m_ValueLabel = new Label(m_Value.ToString());
			m_ValueLabel.AddToClassList("workingMemoryValue");
			hierarchy.Add(m_ValueLabel);
		}

		public static readonly string USS_CLASS_NAME = "workingMemoryItem";

		private readonly string m_Key;
		private object m_Value;

		private Label m_ValueLabel;

	}
}