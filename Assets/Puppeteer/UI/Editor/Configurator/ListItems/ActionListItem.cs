using Puppeteer.Core.Configuration;

namespace Puppeteer.UI
{
	internal class ActionListItem : BasicListItem<ActionDescription>
	{
		public ActionListItem(ActionDescription _description) : base(_description)
		{
		}
	}
}