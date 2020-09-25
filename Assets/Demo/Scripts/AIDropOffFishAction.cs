using Puppeteer.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIDropOffFishAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		_executingAgent.GetWorkingMemory().TryGetValue("PickupTarget", out var value);
		FishManager.Instance.AddFish(value != null ? value as GameObject : null);
	}

	public override void Exit()
	{
		// We don't apply all the effects because the HasEnoughMushrooms effect is only used for planning and actually set by a sensor

		foreach(KeyValuePair<string, object> effect in m_Effects)
		{
			if(effect.Key != HAS_ENOUGH_FISH_KEY)
			{
				m_Agent.AddOrUpdateWorkingMemory(effect.Key, effect.Value);
			}
		}

		base.Exit(false);
	}

	private static readonly string HAS_ENOUGH_FISH_KEY = "HasEnoughFish";
}