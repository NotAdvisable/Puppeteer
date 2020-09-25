using System.Collections.Generic;

namespace Puppeteer.Core.Planning
{
	public interface IAStarPlanner<TNodeType>
	{
		List<TNodeType> GetNeighbours(TNodeType _baseNode);
		float GetDistance(TNodeType _base, TNodeType _end);
	}
}