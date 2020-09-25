using Puppeteer.Core;
using UnityEngine;

public class AISitAroundAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		if (m_Agent.TryGetComponent<Animator>(out var animator))
		{
			animator.SetTrigger(m_StartSittingKey);
		}
	}

	public override void Execute()
	{
		// By doing nothing and not calling the base implementation,
		// this action runs until the action is cancelled from the outside.
	}

	private const string m_StartSittingKey = "StartSitting";
}