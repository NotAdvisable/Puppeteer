using System.Collections.Generic;

namespace Puppeteer.Core.Planning
{
	public class AStar<TAStarNode> : IPathfinder<TAStarNode> where TAStarNode : class, IAStarNode
	{
		public AStar(IAStarPlanner<TAStarNode> _planner)
		{
			if (_planner is null)
			{
				Debug.PuppeteerLogger.Log("AStar needs a valid planner to function.", Debug.LogType.Error);
				return;
			}

			m_Planner = _planner;

			m_OpenSet = new External.FastPriorityQueue<TAStarNode>(128);
			m_ClosedSet = new HashSet<TAStarNode>();
			m_CachedPath = new Stack<TAStarNode>();
		}

		public Stack<TAStarNode> FindPath(TAStarNode _start, TAStarNode _end)
		{
			if (_start.Equals(_end))
			{
				Debug.PuppeteerLogger.Log(string.Format("AStar startNode [{0}] equalled endNode [{1}].", _start.ToString(), _end.ToString()), Debug.LogType.Warning);
				return null;
			}

			if (_start.ReachesEnd(_end))
			{
				return null;
			}

			m_OpenSet.Clear();
			m_ClosedSet.Clear();

			m_OpenSet.Enqueue(_start, 0);
			int counter = 0;
			do
			{
				TAStarNode currentNode = m_OpenSet.Dequeue();
				if (currentNode.ReachesEnd(_end))
				{
					return RetrievePath(currentNode);
				}

				m_ClosedSet.Add(currentNode);

				List<TAStarNode> neighbourNodes = m_Planner.GetNeighbours(currentNode);
				for (int i = 0; i < neighbourNodes.Count; ++i)
				{
					TAStarNode neighbourNode = neighbourNodes[i];
					if (neighbourNode is null || m_ClosedSet.Contains(neighbourNode))
					{
						continue;
					}

					// Assumes that GetNeighbours() returns references to the nodes that are in the openSet
					// so it can update their cost and parent if the route through the current node is better.
					if (m_OpenSet.Contains(neighbourNode))
					{
						float fCostForNeighbour = neighbourNode.GetCost();
						float fCostThroughCurrentNode = currentNode.GetCost() + m_Planner.GetDistance(neighbourNode, _end);

						if (fCostThroughCurrentNode < fCostForNeighbour)
						{
							neighbourNode.SetParent(currentNode);
							neighbourNode.CalculateCosts();

							m_OpenSet.UpdatePriority(neighbourNode, neighbourNode.GetCost());
						}
					}
					else
					{
						neighbourNode.SetParent(currentNode);
						neighbourNode.CalculateCosts();

						m_OpenSet.Enqueue(neighbourNode, neighbourNode.GetCost());
					}
				}
			} while (!m_OpenSet.Empty() && ++counter < 64);

			// Exhausted all possible nodes and couldn't find a valid path.
			return null;
		}

		public IEnumerable<TAStarNode> GetClosedNodes()
		{
			return m_ClosedSet;
		}

		public IEnumerable<TAStarNode> GetOpenNodes()
		{
			return m_OpenSet;
		}

		private Stack<TAStarNode> RetrievePath(TAStarNode _endNode)
		{
			m_CachedPath.Clear();
			TAStarNode currentNode = _endNode;
			do
			{
				m_CachedPath.Push(currentNode);
				currentNode = (TAStarNode)currentNode.GetParent();
			} while (currentNode != null);

			return m_CachedPath;
		}

		private readonly Stack<TAStarNode> m_CachedPath;
		private readonly HashSet<TAStarNode> m_ClosedSet;
		private readonly External.FastPriorityQueue<TAStarNode> m_OpenSet;
		private readonly IAStarPlanner<TAStarNode> m_Planner;
	}
}