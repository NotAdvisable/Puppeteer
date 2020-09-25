using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("Puppeteer.UI", "pui")]

namespace Puppeteer.UI
{
	internal class TabView : VisualElement
	{
		public TabView()
		{
		}

		public void AddNewBasedOnSelection()
		{
			m_SelectedTab.AddNewBasedOnSelection();
		}

		public void AddTab(PuppeteerView _puppeteerView)
		{
			_puppeteerView.AddToClassList("puppeteerTab");
			_puppeteerView.AddToClassList("topRounded");

			TextElement textElement = new TextElement
			{
				name = "text",
				text = _puppeteerView.GetLabel(),
				pickingMode = PickingMode.Ignore,
			};

			textElement.AddToClassList("puppeteerTabText");

			_puppeteerView.Add(textElement);
			_puppeteerView.OnMouseDown += SwitchSelected;
			_puppeteerView.OnListItemDeleted = TabListItemDeleted;

			_puppeteerView.RegisterTabViewCallbacks(this);

			_puppeteerView.UpdateEntryName += _newTabName => textElement.text = _newTabName;

			Add(_puppeteerView);
			m_Tabs.Add(_puppeteerView);
		}

		public void CleanUpAllTabs()
		{
			m_Tabs.ForEach(_tab => _tab.CleanUp());
		}

		public void ClearSelection()
		{
			m_SelectedTab.ClearSelection();
		}

		public void DeleteSelection()
		{
			m_SelectedTab.DeleteSelection();
		}

		public void DuplicateSelection()
		{
			m_SelectedTab.DuplicateSelection();
		}

		public void SelectFirst()
		{
			SwitchSelected(m_Tabs?[0]);
		}

		public void Enable()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleTabView"));
		}

		public void EnterSelection()
		{
			m_SelectedTab.EnterSelection();
		}

		public T GetTabOfType<T>() where T : PuppeteerView
		{
			return m_Tabs.First(_entry => _entry is T) as T;
		}

		public void MoveSelection(MoveDirection _direction)
		{
			m_SelectedTab.MoveSelection(_direction);
		}

		public void OpenEntryInTabOfType<T>(SerialisableGuid _serialisableGuid) where T : PuppeteerView
		{
			OpenEntryInTabOfType<T>(_serialisableGuid);
		}

		public void OpenEntryInTabOfType<T>(Guid _guid) where T : PuppeteerView
		{
			var tabOfType = GetTabOfType<T>();
			SwitchSelected(tabOfType);
			m_SelectedTab.TryOpenEntry(_guid);
		}

		public void OpenEntryInTabOfType<T>(int _objectInstanceID) where T : AgentView
		{
			var tabOfType = GetTabOfType<T>();
			SwitchSelected(tabOfType);
			(m_SelectedTab as T).TryOpenEntry(_objectInstanceID);
		}

		public void OpenTabOfType<T>() where T : PuppeteerView
		{
			var tabOfType = GetTabOfType<T>();
			SwitchSelected(tabOfType);
		}

		public void SaveAllTabs()
		{
			m_Tabs.ForEach(_tab => _tab.SaveAllChanges());
		}

		public void SaveCurrentTab()
		{
			m_SelectedTab.SaveAllChanges();
		}

		private void SwitchSelected(PuppeteerView _selected)
		{
			if (_selected == null || _selected == m_SelectedTab)
			{
				return;
			}

			if (m_SelectedTab != null)
			{
				m_SelectedTab.CloseView();
				m_SelectedTab.RemoveFromClassList("tabSelected");
			}

			m_SelectedTab = _selected;
			OnTabListItemSelected?.Invoke(_selected != null ? _selected.GetType().AssemblyQualifiedName : string.Empty);
			m_SelectedTab.AddToClassList("tabSelected");
			m_SelectedTab.OpenView(m_ConfiguratorWindow.GetConfiguratorStates());
		}

		public void SetConfiguratorWindow(PuppeteerConfiguratorWindow _configuratorWindow)
		{
			m_ConfiguratorWindow = _configuratorWindow;
		}

		public PuppeteerConfiguratorWindow GetPuppeteerConfiguratorWindow()
		{
			return m_ConfiguratorWindow;
		}

		private void TabListItemDeleted(ListItem _deletedListItem)
		{
			OnTabListItemDeleted?.Invoke(_deletedListItem);
		}

		public Action<ListItem> OnTabListItemDeleted;
		public Action<string> OnTabListItemSelected;

		private readonly List<PuppeteerView> m_Tabs = new List<PuppeteerView>();
		private PuppeteerView m_SelectedTab = null;

		private PuppeteerConfiguratorWindow m_ConfiguratorWindow;

		public new class UxmlFactory : UxmlFactory<TabView>
		{
		}
	}
}