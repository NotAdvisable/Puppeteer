using Puppeteer.Core;
using Puppeteer.Core.Debug;

namespace Puppeteer
{
	public class DefaultAction : Core.PuppeteerExecutableAction
	{
		public override void Enter(PuppeteerAgent _executingAgent)
		{
			PuppeteerLogger.Log(string.Format("{0} tries to use the PuppeteerDefault action. This won't have any effect.", _executingAgent), Core.Debug.LogType.Warning);
		}
	}
}