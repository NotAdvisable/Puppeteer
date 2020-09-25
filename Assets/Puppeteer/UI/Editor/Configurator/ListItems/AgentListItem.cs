using Puppeteer.Core;

namespace Puppeteer.UI
{
	public class AgentListItem : ListItem
	{
		public AgentListItem(PuppeteerAgent _agent)
		{
			m_Agent = _agent;
			m_Text = m_Agent.gameObject.name;
			Init();
		}

		public PuppeteerAgent GetAgent()
		{
			return m_Agent;
		}

		private readonly PuppeteerAgent m_Agent;
	}
}