namespace Puppeteer.Core.Planning
{
	public class PlannerHierarchyNodePair<TKey, TValue>
	{
		public HierarchyNode<TKey, TValue> HierarchyNodeInstance = null;
		public PlannerNode<TKey, TValue> PlannerNodeInstance = null;
	}
}