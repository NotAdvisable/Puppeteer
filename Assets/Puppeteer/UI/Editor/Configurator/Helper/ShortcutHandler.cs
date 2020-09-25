using Boo.Lang;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class ShortcutHandler
	{
		public void AddShortcut(Shortcut _shortCut)
		{
			m_ShortCuts.Add(_shortCut);
		}

		public void AddShortcuts(Shortcut[] _shortCuts)
		{
			for (int i = 0; i < _shortCuts.Length; ++i)
			{
				m_ShortCuts.Add(_shortCuts[i]);
			}
		}

		public void ExecuteForEachShortcut(KeyDownEvent _event)
		{
			if (m_KeyDownLock)
			{
				return;
			}

			foreach (Shortcut item in m_ShortCuts)
			{
				if (_event.keyCode != item.MainKey
					|| _event.altKey != item.AltKey
					|| _event.shiftKey != item.ShiftKey
					|| _event.ctrlKey != item.CtrlKey)
				{
					continue;
				}

				m_KeyDownLock = true;
				item.OnKeyDown?.Invoke();
				if (item.StopEventPropagation)
				{
					_event.StopPropagation();
				}
			}
		}

		public void UnlockKeyDown()
		{
			m_KeyDownLock = false;
		}

		public class Shortcut
		{
			public bool AltKey = false;
			public bool CtrlKey = false;
			public KeyCode MainKey = default;
			public Action OnKeyDown = null;
			public bool ShiftKey = false;
			public bool StopEventPropagation = false;
		}

		private bool m_KeyDownLock = false;
		private List<Shortcut> m_ShortCuts = new List<Shortcut>();
	}
}