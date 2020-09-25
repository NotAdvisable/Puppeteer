using Puppeteer.Core.Configuration;

namespace Puppeteer.UI
{
	internal class GoalListItem : BasicListItem<GoalDescription>
	{
		public GoalListItem(GoalDescription _description) : base(_description)
		{
		}
	}
}