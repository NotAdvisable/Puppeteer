using System.Collections.Generic;

namespace Puppeteer.Core.Planning
{
	public interface IPathfinder<TNodeType>
	{
		Stack<TNodeType> FindPath(TNodeType _start, TNodeType _end);

		IEnumerable<TNodeType> GetClosedNodes();

		IEnumerable<TNodeType> GetOpenNodes();

	}
}