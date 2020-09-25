using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal abstract class PuppeteerView : VisualElement
	{
		public PuppeteerView(VisualElement _rootElement, VisualElement _leftPanel, VisualElement _rightPanel)
		{
			m_RootElement = _rootElement;
			m_LeftPanel = _leftPanel;
			m_ListItemScrollViewHeader = m_LeftPanel.Q<TextElement>(name: "scrollViewHeader");
			m_ListItemScrollViewHeaderIcon = m_LeftPanel.Q<Image>(className: "scrollViewHeaderIcon");
			m_ListItemScrollView = m_LeftPanel.Q<VisualElement>(className: "scrollView");

			m_AddButton = m_LeftPanel.Q<Button>(name: "addButton");

			m_RightPanel = _rightPanel;
			m_RightPanelContent = m_RightPanel.Q<VisualElement>(className: "rightPanelContent");
			m_ConfiguratorElement = new VisualElement();

			m_ConfiguratorElement.AddToClassList("rightPanelContent");

			m_WarningText = m_RightPanel.Q<TextElement>(className: "warningText");

			this.AddManipulator(new Clickable(() => OnMouseDown?.Invoke(this)));
		}

		public virtual void AddNewBasedOnSelection()
		{
		}

		public virtual void CleanUp()
		{
		}

		public virtual void ClearSelection()
		{
			if (m_SelectedListItem != null)
			{
				m_SelectedListItem.RemoveFromClassList("selected");
				m_SelectedListItem = null;
				OnListItemSelectedOrRemoved?.Invoke(m_SelectedListItem);
				DisableRightPanelContent();
			}
		}

		public virtual void CloseView()
		{
			m_ConfiguratorElement.RemoveFromHierarchy();

			DisableRightPanelContent();
			m_ListItemScrollView.Clear();
			m_ListItemScrollViewHeader.text = string.Empty;
			m_ListItemScrollViewHeaderIcon.visible = false;

			m_IsOpen = false;
		}

		public void DeleteListItem<T>(T _listItem) where T : ListItem
		{
			if (_listItem == m_SelectedListItem)
			{
				bool neighbourSelected = SelectNeighbourListItem(MoveDirection.Up);
				if (!neighbourSelected)
				{
					neighbourSelected |= SelectNeighbourListItem(MoveDirection.Down);
				}

				if (!neighbourSelected)
				{
					m_SelectedListItem = null;
					OnListItemSelectedOrRemoved?.Invoke(m_SelectedListItem);
					DisableRightPanelContent();
				}
			}

			_listItem.RemoveFromHierarchy();
			m_ListItems.Remove(_listItem);
			AddUnsavedMarker();

			OnListItemDeleted?.Invoke(_listItem);
		}

		public virtual void DeleteSelection()
		{
			if (m_SelectedListItem != null)
			{
				DeleteListItem(m_SelectedListItem);
			}
		}

		public virtual void DuplicateSelection()
		{
		}

		public virtual void EnterSelection()
		{
		}

		public string GetLabel()
		{
			return m_Label;
		}

		public virtual void MoveSelection(MoveDirection _direction)
		{
			if (m_SelectedListItem != null)
			{
				SelectNeighbourListItem(_direction);
			}
		}

		public virtual void OpenView(SerialisedConfiguratorState _serialisedConfiguratorStates)
		{
			m_RightPanelContent.Add(m_ConfiguratorElement);
			m_ListItemScrollViewHeaderIcon.visible = true;

			m_IsOpen = true;
		}

		public virtual void RegisterTabViewCallbacks(TabView _tabView)
		{
		}

		public virtual void SaveAllChanges()
		{
		}

		public virtual void SaveSelectedChange()
		{
		}

		public virtual bool TryOpenEntry(Guid _guid)
		{
			return false;
		}

		public void UpdateSelectedListItem<T>(T _listItem) where T : ListItem
		{
			if (m_SelectedListItem == _listItem)
			{
				return;
			}

			if (m_SelectedListItem != null)
			{
				m_SelectedListItem.RemoveFromClassList("selected");
				OnListItemDeselected?.Invoke(m_SelectedListItem);
			}

			m_SelectedListItem = _listItem;
			m_SelectedListItem?.AddToClassList("selected");
			OnListItemSelectedOrRemoved?.Invoke(m_SelectedListItem);

			UpdateConfigurator();
		}

		internal void AddUnsavedMarker()
		{
			UpdateEntryName?.Invoke(GetLabel() + UNSAVED_CHANGES_POSTFIX);
		}

		protected void DisableRightPanelContent()
		{
			if (m_RightPanelContent.parent != null)
			{
				m_RightPanelContent.RemoveFromHierarchy();
			}
			if (m_WarningText.parent == null)
			{
				m_RightPanel.Add(m_WarningText);
			}
		}

		protected void EnableRightPanelContent()
		{
			if (m_RightPanelContent.parent == null)
			{
				m_RightPanel.Add(m_RightPanelContent);
			}
			if (m_WarningText.parent != null)
			{
				m_WarningText.RemoveFromHierarchy();
			}
		}

		protected abstract void LazyInitConfigurator();

		protected bool SelectNeighbourListItem(MoveDirection _direction)
		{
			int index = m_ListItems.IndexOf(m_SelectedListItem);
			if (index > 0 && _direction == MoveDirection.Up)
			{
				UpdateSelectedListItem(m_ListItems[--index]);
				return true;
			}
			else if (index < m_ListItems.Count - 1 && _direction == MoveDirection.Down)
			{
				UpdateSelectedListItem(m_ListItems[++index]);
				return true;
			}

			return false;
		}

		protected void TryClearUnsavedMarker()
		{
			var allListItems = m_ListItemScrollView.Query<ListItem>(className: "listItem").ToList();

			if (!allListItems.Any(_entry => _entry.IsMarkedForUnsavedChanges()))
			{
				UpdateEntryName?.Invoke(GetLabel());
			}
		}

		protected abstract void UpdateConfigurator();

		public Action<ListItem> OnListItemDeleted;
		public Action<ListItem> OnListItemDeselected;
		public Action<ListItem> OnListItemSelectedOrRemoved;
		public Action<PuppeteerView> OnMouseDown;
		public Action<string> UpdateEntryName;

		protected readonly Button m_AddButton;
		protected readonly VisualElement m_ConfiguratorElement;
		protected readonly VisualElement m_LeftPanel;
		protected readonly VisualElement m_ListItemScrollView;
		protected readonly TextElement m_ListItemScrollViewHeader;
		protected readonly Image m_ListItemScrollViewHeaderIcon;
		protected readonly VisualElement m_RightPanel;
		protected readonly VisualElement m_RightPanelContent;
		protected readonly VisualElement m_RootElement;
		protected readonly TextElement m_WarningText;

		protected bool m_IsOpen = false;
		protected string m_Label;
		protected List<ListItem> m_ListItems = new List<ListItem>();
		protected ListItem m_SelectedListItem;

		private static readonly string UNSAVED_CHANGES_POSTFIX = "*";
	}
}