using Puppeteer.Core.WorldState;

namespace Puppeteer.Core.Planning
{
	public class GoalPlanPair<TKey, TValue>
	{
		public Goal<TKey, TValue> GoalInstance;
		public Plan<TKey, TValue> PlanInstance;
	}
}