using Puppeteer.Core.Action;
using Puppeteer.Core.Planning;
using Puppeteer.Core.WorldState;
using System.Collections.Generic;
using System.Linq;

namespace Puppeteer.Core.Helper
{
	public static class PuppeteerRuntimeHelper
	{
		public static int AddAllToHashSet<T>(ref HashSet<T> _hashSet, T[] _array)
		{
			int i = 0;
			for (; i < _array.Length; ++i)
			{
				_hashSet.Add(_array[i]);
			}

			return i;
		}

		public static void ConstructHierarchy<TKey, TValue>(ref List<PlannerHierarchyNodePair<TKey, TValue>> _nodePairs)
		{
			foreach (var currentNodePair in _nodePairs)
			{
				var directChildren = _nodePairs.Where(_entry => _entry.PlannerNodeInstance.GetParent() == currentNodePair.PlannerNodeInstance);

				foreach (var directChildPair in directChildren)
				{
					directChildPair.HierarchyNodeInstance.SetParent(currentNodePair.HierarchyNodeInstance);
					currentNodePair.HierarchyNodeInstance.AddChild(directChildPair.HierarchyNodeInstance);
				}
			}
		}

		public static void CreateNodePairForLastOpenNode<TKey, TValue>(ref PlannerNode<TKey, TValue> _lastPlannerNode, ref List<PlannerHierarchyNodePair<TKey, TValue>> _nodePairs)
		{
			var lastHierarchyNode = new HierarchyNode<TKey, TValue>(
				_isPartOfFoundPath: true,
				_executableAction: _lastPlannerNode.Action as ExecutableAction<TKey, TValue>,
				_goalWorldStateAtNode: _lastPlannerNode.GetGoalWorldStateAtNode(),
				_worldStateAtNode: _lastPlannerNode.GetWorldStateAtNode(),
				_isFinalNode: true,
				_isClosed: false);

			_nodePairs.Add(new PlannerHierarchyNodePair<TKey, TValue>() { PlannerNodeInstance = _lastPlannerNode, HierarchyNodeInstance = lastHierarchyNode });
		}

		public static void CreateNodePairsForClosedNodes<TKey, TValue>(IEnumerable<PlannerNode<TKey, TValue>> _closedNodes, ref List<PlannerHierarchyNodePair<TKey, TValue>> _nodePairs)
		{
			CreateNodePairsForPlannerNodes(_closedNodes, ref _nodePairs, true);
		}

		public static void CreateNodePairsForOpenNodes<TKey, TValue>(IEnumerable<PlannerNode<TKey, TValue>> _openNodes, ref List<PlannerHierarchyNodePair<TKey, TValue>> _nodePairs)
		{
			CreateNodePairsForPlannerNodes(_openNodes, ref _nodePairs, false);
		}

		public static void SetPartOfFoundPathRecursive<TKey, TValue>(HierarchyNode<TKey, TValue> _node)
		{
			_node.SetIsPartOfFoundPath(true);
			if (_node.GetParent() != null)
			{
				SetPartOfFoundPathRecursive(_node.GetParent());
			}
		}

		private static void CreateNodePairsForPlannerNodes<TKey, TValue>(IEnumerable<PlannerNode<TKey, TValue>> _plannerNodes, ref List<PlannerHierarchyNodePair<TKey, TValue>> _nodePairs, bool _isClosed)
		{
			foreach (var plannerNode in _plannerNodes)
			{
				var nodePairAtSameState = _nodePairs.Find(_entry => _entry.PlannerNodeInstance == plannerNode);

				HierarchyNode<TKey, TValue> hierarchyNode = null;
				if (nodePairAtSameState == null)
				{
					hierarchyNode = new HierarchyNode<TKey, TValue>(
						_isPartOfFoundPath: false,
						_executableAction: plannerNode.Action as ExecutableAction<TKey, TValue>,
						_goalWorldStateAtNode: plannerNode.GetGoalWorldStateAtNode(),
						_worldStateAtNode: plannerNode.GetWorldStateAtNode(),
						_isFinalNode: false,
						_isClosed: _isClosed);
				}
				else
				{
					hierarchyNode = nodePairAtSameState.HierarchyNodeInstance;
				}

				_nodePairs.Add(new PlannerHierarchyNodePair<TKey, TValue>() { PlannerNodeInstance = plannerNode, HierarchyNodeInstance = hierarchyNode });
			}
		}
	}
}