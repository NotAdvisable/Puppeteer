using Puppeteer.Core.External;

namespace Puppeteer.Core.WorldState
{
	public class SortableGoal<TKey, TValue> : Goal<TKey, TValue>, IFastPriorityQueueNode
	{
		public float Priority { get; set; }
		public int QueueIndex { get; set; }
		public object Queue { get; set; }
	}
}

