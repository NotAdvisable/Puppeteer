using Puppeteer.Core.WorldState;

namespace Puppeteer.Core
{
	public class PuppeteerGoal : SortableGoal<string, object>
	{
		public System.Guid DescriptionGUID { get; set; }
	}
}