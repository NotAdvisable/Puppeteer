using Puppeteer.Core.Action;
using Puppeteer.Core.WorldState;
using System.Collections.Generic;
using System.Linq;

namespace Puppeteer.Core.Planning
{
	public class HierarchyNode<TKey, TValue>
	{
		public HierarchyNode(bool _isPartOfFoundPath, ExecutableAction<TKey, TValue> _executableAction, WorldState<TKey, TValue> _worldStateAtNode,
			WorldState<TKey, TValue> _goalWorldStateAtNode, bool _isFinalNode, bool _isClosed)
		{
			m_IsPartOfFoundPath = _isPartOfFoundPath;
			m_ExecutableAction = _executableAction;
			m_IsFinalNode = _isFinalNode;
			m_IsClosed = _isClosed;

			m_SortedGoalWorldStateAtNode = CreateSortedWorldStateList(ref _goalWorldStateAtNode);

			WorldState<TKey, TValue> totalRemainingPreconditions = _goalWorldStateAtNode - _worldStateAtNode;

			m_SortedRemainingPreconditions = CreateSortedWorldStateList(ref totalRemainingPreconditions);
		}

		public void AddChild(HierarchyNode<TKey, TValue> _childNode)
		{
			if (m_Children == null)
			{
				m_Children = new List<HierarchyNode<TKey, TValue>>();
			}

			m_Children.Add(_childNode);
		}

		public List<KeyValuePair<TKey, TValue>> CreateSortedWorldStateList(ref WorldState<TKey, TValue> _worldState)
		{
			var puppeteerWorldStateList = new List<KeyValuePair<TKey, TValue>>();
			foreach (KeyValuePair<TKey, TValue> _entry in _worldState)
			{
				puppeteerWorldStateList.Add(new KeyValuePair<TKey, TValue>(_entry.Key, _entry.Value));
			}

			puppeteerWorldStateList = puppeteerWorldStateList.OrderBy(_entry => _entry.Key).ToList();
			return puppeteerWorldStateList;
		}

		public int GetChildCount()
		{
			return m_Children != null ? m_Children.Count : 0;
		}

		public List<HierarchyNode<TKey, TValue>> GetChildren()
		{
			return m_Children;
		}

		public ExecutableAction<TKey, TValue> GetExecutableAction()
		{
			return m_ExecutableAction;
		}

		public HierarchyNode<TKey, TValue> GetParent()
		{
			return m_Parent;
		}

		public List<KeyValuePair<TKey, TValue>> GetSortedGoalWorldStateAtNode()
		{
			return m_SortedGoalWorldStateAtNode;
		}

		public List<KeyValuePair<TKey, TValue>> GetSortedRemainingPreconditions()
		{
			return m_SortedRemainingPreconditions;
		}

		public bool IsClosed()
		{
			return m_IsClosed;
		}

		public bool IsFinalNode()
		{
			return m_IsFinalNode;
		}

		public bool IsPartOfFoundPath()
		{
			return m_IsPartOfFoundPath;
		}

		public void SetIsPartOfFoundPath(bool _value)
		{
			m_IsPartOfFoundPath = _value;
		}

		public void SetParent(HierarchyNode<TKey, TValue> _parentNode)
		{
			m_Parent = _parentNode;
		}

		private readonly ExecutableAction<TKey, TValue> m_ExecutableAction = null;
		private readonly bool m_IsClosed = false;
		private readonly bool m_IsFinalNode = false;
		private readonly List<KeyValuePair<TKey, TValue>> m_SortedGoalWorldStateAtNode;
		private readonly List<KeyValuePair<TKey, TValue>> m_SortedRemainingPreconditions;

		private List<HierarchyNode<TKey, TValue>> m_Children = null;
		private bool m_IsPartOfFoundPath = false;
		private HierarchyNode<TKey, TValue> m_Parent = null;
	}
}