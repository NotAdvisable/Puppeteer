using Puppeteer.Core.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class SearchPopup<TItem> : VisualElement where TItem : BasicDescription
	{
		public SearchPopup(VisualElement _rootElement, List<TItem> _listItems, Vector2 _position, Vector2 _parentMinMaxX)
		{
			style.position = Position.Absolute;

			style.maxWidth = MAX_WIDTH;
			style.maxHeight = MAX_HEIGHT;

			float xPosition;

			if (_position.x + MAX_WIDTH < _parentMinMaxX.y)
			{
				xPosition = _position.x;
			}
			else
			{
				if (_position.x - MAX_WIDTH > _parentMinMaxX.x)
				{
					xPosition = _position.x - MAX_WIDTH;
				}
				else
				{
					float DifferenceToMax = _position.x + MAX_WIDTH - _parentMinMaxX.y;

					xPosition = _position.x - (DifferenceToMax + 10);
				}
			}

			style.top = _position.y;
			style.bottom = 0;
			style.left = xPosition;
			style.right = 0;

			AddToClassList(USS_CLASS_NAME);
			AddToClassList("lightBackground");

			m_SearchField.AddToClassList(USS_CLASS_NAME_SEARCHFIELD);
			Add(m_SearchField);
			m_SearchField.RegisterCallback<ChangeEvent<string>>(SearchFieldChanged);

			foreach (var item in _listItems)
			{
				var basicListItem = new BasicListItem<TItem>(_description: item,
										_useDeleteContextManipulator: false,
										_useDuplicateManipulator: false);

				m_ListItems.Add(basicListItem);
			}

			m_ListItems = m_ListItems.OrderBy(_entry => _entry.GetDescription().DisplayName).ToList();

			SearchFieldChanged(new ChangeEvent<string>());

			m_ScrollView.AddToClassList(USS_CLASS_NAME_SCROLLVIEW);
			Add(m_ScrollView);

			m_RootElement = _rootElement;
			m_RootElement.RegisterCallback<MouseDownEvent>(ClickedInWindow);
			m_RootElement.RegisterCallback<GeometryChangedEvent>(WindowGeometryChanged);
		}

		public void ChooseSelected()
		{
			if (parent == null)
			{
				m_SelectedItem = null;
			}

			m_SelectedItem?.OnMouseDown?.Invoke(m_SelectedItem);
		}

		public void FocusSearchField()
		{
			// m_SearchField.Focus();

			m_SearchField.Q("unity-text-input").Focus();
			// This took way too long to figure out..
			// https://issuetracker.unity3d.com/issues/uielements-textfield-is-not-focused-and-you-are-not-able-to-type-in-characters-when-using-focus-method
		}

		public void MoveSelection(MoveDirection _direction)
		{
			SelectNeighbourItem(_direction);
		}

		private void ClickedInWindow(MouseDownEvent _event)
		{
			if (parent == null)
			{
				m_RootElement?.UnregisterCallback<MouseDownEvent>(ClickedInWindow);
			}

			if (!ContainsPoint(this.WorldToLocal(_event.mousePosition)))
			{
				m_RootElement.UnregisterCallback<MouseDownEvent>(ClickedInWindow);
				RemoveFromHierarchy();
			}
		}

		private void ListItemClicked(ListItem _item)
		{
			OnListItemClicked?.Invoke(_item);
			RemoveFromHierarchy();
		}

		private void SearchFieldChanged(ChangeEvent<string> _event)
		{
			m_ScrollView.Clear();

			if (_event.newValue != null)
			{
				var filtered = m_ListItems.FindAll(_entry => _entry.GetDescription().DisplayName.ToLower().Contains(_event.newValue.ToLower()));
				foreach (var item in filtered)
				{
					m_ScrollView.Add(item);
					item.OnMouseDown = ListItemClicked;
				}
			}
			else
			{
				foreach (var item in m_ListItems)
				{
					m_ScrollView.Add(item);
					item.OnMouseDown = ListItemClicked;
				}
			}

			if (m_ScrollView.childCount > 0)
			{
				UpdateSelectedItem(m_ScrollView[0] as BasicListItem<TItem>);
			}
		}

		private bool SelectNeighbourItem(MoveDirection _direction)
		{
			if (_direction != MoveDirection.Up && _direction != MoveDirection.Down)
			{
				return false;
			}
			int index = m_ScrollView.IndexOf(m_SelectedItem);

			if (index > 0 && _direction == MoveDirection.Up)
			{
				UpdateSelectedItem(m_ScrollView[--index] as BasicListItem<TItem>);
				return true;
			}
			else if (index < m_ScrollView.childCount - 1 && _direction == MoveDirection.Down)
			{
				UpdateSelectedItem(m_ScrollView[++index] as BasicListItem<TItem>);
				return true;
			}

			return false;
		}

		private void UpdateSelectedItem(BasicListItem<TItem> _item)
		{
			if (m_SelectedItem != null)
			{
				m_SelectedItem.RemoveFromClassList("selected");
			}

			_item.AddToClassList("selected");
			m_SelectedItem = _item;
		}

		private void WindowGeometryChanged(GeometryChangedEvent _event)
		{
			m_RootElement.UnregisterCallback<GeometryChangedEvent>(WindowGeometryChanged);
			RemoveFromHierarchy();
		}
		public static readonly string USS_CLASS_NAME = "searchPopup";
		public static readonly string USS_CLASS_NAME_SCROLLVIEW = USS_CLASS_NAME + "ScrollView";
		public static readonly string USS_CLASS_NAME_SEARCHFIELD = USS_CLASS_NAME + "SearchField";
		public System.Action<ListItem> OnListItemClicked;
		private static readonly float MAX_HEIGHT = 300;
		private static readonly float MAX_WIDTH = 305;
		private readonly List<BasicListItem<TItem>> m_ListItems = new List<BasicListItem<TItem>>();
		private VisualElement m_RootElement = null;
		private ScrollView m_ScrollView = new ScrollView();
		private SearchField m_SearchField = new SearchField();
		private BasicListItem<TItem> m_SelectedItem = null;
	}
}