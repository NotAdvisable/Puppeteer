using Puppeteer.Core.Planning;
using Puppeteer.Core.Sensor;

namespace Puppeteer.Core
{
	public class PuppeteerExecutableSensor : ExecutableSensor<string, object>
	{
		public System.Guid DescriptionGUID { get; set; }

		public virtual bool DetectWorldStateChange(PuppeteerAgent _owner)
		{
			return false;
		}

		public override sealed bool DetectWorldStateChange(IAgent<string, object> _owner)
		{
			return DetectWorldStateChange(_owner as PuppeteerAgent);
		}

		public override sealed void Initialise(Agent<string, object> _owner)
		{
			Initialise(_owner as PuppeteerAgent);
		}

		public virtual void Initialise(PuppeteerAgent _owner)
		{
		}
	}
}