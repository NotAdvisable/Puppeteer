using Puppeteer.Core.Action;
using Puppeteer.Core.WorldState;
using System.Collections.Generic;
using System.Linq;
namespace Puppeteer.Core.Planning
{
	public class PlannerNode<TKey, TValue> : IAStarNode
	{
		public PlannerNode()
		{
		}

		public void Init(IAction<TKey, TValue> _action)
		{
			m_Action = _action;
		}

		public void Reset()
		{
			m_Action = null;
			
			Priority = 0;
			Queue = null;
			QueueIndex = 0;
			
			m_PathCost = 0.0f;
			m_HeuristicCost = 0.0f;
			m_Parent = null;

			m_WorldStateAtNode = null;
			m_GoalWorldStateAtNode = null;
		}

		public float Priority { get; set; }

		public object Queue { get; set; }

		public int QueueIndex { get; set; }
		public IAction<TKey, TValue> Action { get => m_Action; set => m_Action = value; }

		public float GetCost()
		{
			return m_PathCost + m_HeuristicCost;
		}

		public float GetPathCost()
		{
			return m_PathCost;
		}

		public float GetHeuristicCost()
		{
			return m_HeuristicCost;
		}

		public void CalculateCosts()
		{			
			m_HeuristicCost = (m_Action != null)? 1 / m_Action.GetUtility() : float.MaxValue;
			if(m_Parent != null)
			{
				m_PathCost += m_Parent.GetCost();
			}
		}

		public IAStarNode GetParent()
		{
			return m_Parent;
		}

		internal Goal<TKey, TValue> GetGoalWorldStateAtNode()
		{
			return m_GoalWorldStateAtNode;
		}

		public void SetParent(IAStarNode _parentNode)
		{
			m_Parent = _parentNode;
		}

		internal void CalculateWorldStateAtNode(WorldState<TKey, TValue> _baseWorldState)
		{
			m_WorldStateAtNode = WorldState<TKey, TValue>.Copy(_baseWorldState);

			if (m_Action != null)
			{
				m_WorldStateAtNode.Apply(m_Action.GetEffects());
			}
		}

		internal void CalculateGoalWorldStateAtNode(Goal<TKey, TValue> _baseGoalWorldState)
		{
			m_GoalWorldStateAtNode = Goal<TKey,TValue>.Copy(_baseGoalWorldState);

			if (m_Action != null)
			{
				m_GoalWorldStateAtNode.Apply(m_Action.GetPreconditions());
			}
		}

		internal WorldState<TKey, TValue> GetWorldStateAtNode()
		{
			return m_WorldStateAtNode;
		}

		public bool Equals(IAStarNode _other)
		{
			if (_other is PlannerNode<TKey, TValue> sameType)
			{
				if (!m_WorldStateAtNode.Equals(sameType.m_WorldStateAtNode))
				{
					return false;
				}

				if (m_GoalWorldStateAtNode != null)
				{
					if (sameType.m_GoalWorldStateAtNode == null)
					{
						return false;
					}
					if (!m_GoalWorldStateAtNode.Equals(sameType.m_GoalWorldStateAtNode))
					{
						return false;
					}
				}else if (sameType.m_GoalWorldStateAtNode != null)
				{
					return false;
				}

				if(sameType.m_Action != m_Action)
				{
					return false;
				}

				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("Action: {0}, WorldStateAtNode: {1}, GoalWorldStateAtNode: {2}, PathCost: {3}", 
				m_Action != null ? m_Action.GetLabel() : "{}",
				m_WorldStateAtNode != null ? m_WorldStateAtNode.ToString() : "{}", 
				m_GoalWorldStateAtNode != null ? m_GoalWorldStateAtNode.ToString() : "{}",
				m_PathCost);
		}

		public bool ReachesEnd(IAStarNode _end)
		{
			return m_WorldStateAtNode.ContainsAll(m_GoalWorldStateAtNode);
		}

		public override int GetHashCode()
		{
			int hashCode = 2126105136;
			hashCode = hashCode * -1521134295 + EqualityComparer<WorldState<TKey, TValue>>.Default.GetHashCode(m_WorldStateAtNode);
			hashCode = hashCode * -1521134295 + EqualityComparer<WorldState<TKey, TValue>>.Default.GetHashCode(m_GoalWorldStateAtNode);
			hashCode = hashCode * -1521134295 + EqualityComparer<IAction<TKey, TValue>>.Default.GetHashCode(m_Action);
			return hashCode;
		}

		private WorldState<TKey, TValue> m_WorldStateAtNode = null;
		private Goal<TKey, TValue> m_GoalWorldStateAtNode = null;

		private IAction<TKey, TValue> m_Action = null;
		private IAStarNode m_Parent = null;

		private float m_PathCost = 0.0f;
		private float m_HeuristicCost = 0.0f;
	}
}