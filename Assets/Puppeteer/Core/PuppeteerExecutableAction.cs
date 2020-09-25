using Puppeteer.Core.Action;
using Puppeteer.Core.WorldState;

namespace Puppeteer.Core
{
	public abstract class PuppeteerExecutableAction : ExecutableAction<string, object>
	{
		public System.Guid DescriptionGUID { get; set; }

		public sealed override void Enter(Agent<string, object> _executingAgent)
		{
			Enter(_executingAgent as PuppeteerAgent);
		}

		public virtual void Enter(PuppeteerAgent _executingAgent)
		{
			base.Enter(_executingAgent);
			m_Agent = _executingAgent;
		}

		public void Exit(bool _applyAllEffects = true) 
		{
			if (_applyAllEffects)
			{
				m_Agent.GetWorkingMemory().Apply(m_Effects);
			}

			base.Exit();
		}

		public override void Exit()
		{
			Exit(true);
		}

		public sealed override bool IsValid(Agent<string, object> _executingAgent)
		{
			return IsValid(_executingAgent);
		}

		public virtual bool IsValid(PuppeteerAgent _executingAgent)
		{
			return true;
		}

		public void SetBaseUtility(float _baseUtility)
		{
			m_BaseUtility = _baseUtility;
		}

		public void SetEffects(WorldStateModifier<string, object> _effects)
		{
			m_Effects = _effects;
		}

		public void SetLabel(string _label)
		{
			m_Label = _label;
		}

		public void SetPreconditions(WorldStateModifier<string, object> _preconditons)
		{
			m_Preconditions = _preconditons;
		}

		protected PuppeteerAgent m_Agent;
	}
}