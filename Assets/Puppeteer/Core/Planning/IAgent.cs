using Puppeteer.Core.Action;
using Puppeteer.Core.External;
using Puppeteer.Core.WorldState;
using System.Collections.Generic;

namespace Puppeteer.Core.Planning
{
	public interface IAgent<TKey, TValue>
	{
		void AddAction(IAction<TKey, TValue> _action);

		FastPriorityQueue<SortableGoal<TKey, TValue>> CalculateSortedGoals();

		void ClearPlanHierarchy();

		void CreatePlanHierarchy(PlannerNode<TKey, TValue> _finalPathNode, IEnumerable<PlannerNode<TKey, TValue>> _openNodes, IEnumerable<PlannerNode<TKey, TValue>> _closedNodes);

		ActionSet<TKey, TValue> GetActionSet();

		GoalSet<TKey, TValue> GetGoalSet();

		WorldState<TKey, TValue> GetWorkingMemory();

		bool IsGoalBetterThanActiveGoal(Goal<TKey, TValue> _goal);

		void SetActiveGoal(Goal<TKey, TValue> _goal);

		void SetWorkingMemory(WorldState<TKey, TValue> _worldState);
	}
}