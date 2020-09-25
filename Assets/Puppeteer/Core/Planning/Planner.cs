using Puppeteer.Core.Action;
using Puppeteer.Core.Planning;
using Puppeteer.Core.WorldState;
using System.Collections.Generic;
using System.Linq;

namespace Puppeteer.Core
{
	public class Planner<TKey, TValue> : IAStarPlanner<PlannerNode<TKey, TValue>>
	{
		public Planner()
		{
			m_Pathfinder = new AStar<PlannerNode<TKey, TValue>>(this);
			m_NeighbourNodes = new List<PlannerNode<TKey, TValue>>();
			m_UsedNodes = new List<PlannerNode<TKey, TValue>>();

			int plannerNodeCacheSize = 512;
			m_CachedPlannerNodes = new Stack<PlannerNode<TKey, TValue>>(plannerNodeCacheSize);
			for (int i = 0; i < plannerNodeCacheSize; ++i)
			{
				m_CachedPlannerNodes.Push(new PlannerNode<TKey, TValue>());
			}
		}

		public GoalPlanPair<TKey, TValue> GenerateGoalPlanPairForAgent(IAgent<TKey, TValue> _agent)
		{
			m_CurrentAgent = _agent;

			Stack<PlannerNode<TKey, TValue>> foundPath = null;
			bool newGoalBetterThanActiveGoal = false;

			m_GoalPlanPair.GoalInstance = null;
			m_GoalPlanPair.PlanInstance = null;

			for(var goalQueue = m_CurrentAgent.CalculateSortedGoals(); goalQueue.Count > 0; )
			{
				SortableGoal<TKey, TValue> goal = goalQueue.Dequeue();

				PlannerNode<TKey, TValue> agentNode = new PlannerNode<TKey, TValue>();
				agentNode.CalculateWorldStateAtNode(_agent.GetWorkingMemory());
				agentNode.CalculateGoalWorldStateAtNode(goal);

				foundPath = m_Pathfinder.FindPath(agentNode, null);
				if (foundPath != null && foundPath.Count > 0)
				{
					newGoalBetterThanActiveGoal = _agent.IsGoalBetterThanActiveGoal(goal);
					if (newGoalBetterThanActiveGoal)
					{
						m_GoalPlanPair.GoalInstance = goal;
#if UNITY_EDITOR
						_agent.CreatePlanHierarchy(foundPath.Last(), m_Pathfinder.GetOpenNodes(), m_Pathfinder.GetClosedNodes());
#endif
					}
					break;
				}
			}

			m_GoalPlanPair.PlanInstance = new Plan<TKey, TValue>();
			if (newGoalBetterThanActiveGoal && foundPath != null)
			{
				foreach (PlannerNode<TKey, TValue> pathNode in foundPath)
				{
					if (pathNode.Action != null)
					{
						m_GoalPlanPair.PlanInstance.Push(pathNode.Action);
					}
				}
			}

			m_UsedNodes.Clear();
			RecycleUsedNodes();

			return m_GoalPlanPair;
		}

		public float GetDistance(PlannerNode<TKey, TValue> _base, PlannerNode<TKey, TValue> _end)
		{
			/// GetNeighbours for this planner already filters out all the nodes that
			/// don't get us closer to the end so we simply return the heuristic cost of the node
			return _base.GetHeuristicCost();
		}

		public List<PlannerNode<TKey, TValue>> GetNeighbours(PlannerNode<TKey, TValue> _baseNode)
		{
			m_NeighbourNodes.Clear();

			WorldStateModifier<TKey, TValue> basePreconditions = _baseNode.Action?.GetPreconditions();
			WorldState<TKey, TValue> worldStateAtBaseNode = _baseNode.GetWorldStateAtNode();
			Goal<TKey, TValue> goalWorldStateAtBaseNode = _baseNode.GetGoalWorldStateAtNode();

			WorldState<TKey, TValue> remainingGoals = goalWorldStateAtBaseNode - worldStateAtBaseNode;

			// We implement regressive A* for GOAP by chaining action preconditions to action effects until all preconditions are fulfilled.
			// For this planner, neighbours are nodes that fulfil any of the remaining goals of the base node without conflicting with any of its preconditions.

			foreach (IAction<TKey, TValue> possibleAction in m_CurrentAgent.GetActionSet())
			{
				var effects = possibleAction.GetEffects();

				if (!effects.ContainsAny(remainingGoals))
				{
					continue;
				}

				if (effects.ConflictsAny(basePreconditions))
				{
					continue;
				}

				PlannerNode<TKey, TValue> plannerNode = GetAvailablePlannerNode();
				plannerNode.Init(possibleAction);

				plannerNode.CalculateWorldStateAtNode(worldStateAtBaseNode);
				plannerNode.CalculateGoalWorldStateAtNode(goalWorldStateAtBaseNode);

				bool foundInUsed = false;
				foreach (PlannerNode<TKey, TValue> usedNode in m_UsedNodes)
				{
					if (usedNode.Equals(plannerNode))
					{
						plannerNode = usedNode;
						foundInUsed = true;
						break;
					}
				}

				m_NeighbourNodes.Add(plannerNode);

				if (!foundInUsed)
				{
					m_UsedNodes.Add(plannerNode);
				}
			}

			return m_NeighbourNodes;
		}

		private PlannerNode<TKey, TValue> GetAvailablePlannerNode()
		{
			return m_CachedPlannerNodes.Count > 0 ? m_CachedPlannerNodes.Pop() : new PlannerNode<TKey, TValue>();
		}

		private void RecycleUsedNodes()
		{
			void RecycleNodes(IEnumerable<PlannerNode<TKey, TValue>> _nodes)
			{
				foreach (var node in _nodes)
				{
					node.Reset();
					m_CachedPlannerNodes.Push(node);
				}
			}

			RecycleNodes(m_Pathfinder.GetClosedNodes());
			RecycleNodes(m_Pathfinder.GetOpenNodes());
		}

		private readonly List<PlannerNode<TKey, TValue>> m_NeighbourNodes;
		private readonly IPathfinder<PlannerNode<TKey, TValue>> m_Pathfinder;
		private readonly List<PlannerNode<TKey, TValue>> m_UsedNodes;
		private readonly Stack<PlannerNode<TKey, TValue>> m_CachedPlannerNodes;
		private readonly GoalPlanPair<TKey, TValue> m_GoalPlanPair = new GoalPlanPair<TKey, TValue>();
		private IAgent<TKey, TValue> m_CurrentAgent;
	}
}