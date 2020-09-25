using System;

namespace Puppeteer.Core.Planning
{
	public interface IAStarNode : External.IFastPriorityQueueNode, IEquatable<IAStarNode>
	{
		/// <summary>
		/// In A* referred to as F. G (path cost) and H (heuristic cost) combined. (f = g + h)
		/// </summary>
		float GetCost();

		/// <summary>
		/// In A* referred to as H. The heuristic cost of the current node.
		/// </summary>
		float GetHeuristicCost();

		/// <summary>
		/// In A* referred to as G. The cost to get to this node.
		/// </summary>
		float GetPathCost();

		/// <summary>
		/// Returns the parent node of this node. Null if it's the first node in the queue.
		/// </summary>
		IAStarNode GetParent();

		/// <summary>
		/// Overrides the parent node. <see cref="CalculateCosts"/> should be called afterwards.
		/// </summary>
		void SetParent(IAStarNode _parentNode);

		/// <summary>
		/// Calculates the cost of this node based on parent and heuristic.
		/// </summary>
		void CalculateCosts();

		bool ReachesEnd(IAStarNode _end);
	}
}