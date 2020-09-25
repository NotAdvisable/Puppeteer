using Puppeteer.Core;
using Puppeteer.Core.Configuration;

namespace Puppeteer.UI
{
	internal class SensorListItem : BasicListItem<SensorDescription>
	{
		public SensorListItem(SensorDescription _description) : base(_description)
		{
		}
	}
}