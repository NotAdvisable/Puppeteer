using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;

public class AIFindFishingSpotAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		Vector3 fishingSpot = FishManager.Instance.GetCurrentFishingSpotPosition();

		if (fishingSpot == null)
		{
			m_CurrentActionState = ActionState.Failed;
			return;
		}

		_executingAgent.AddOrUpdateWorkingMemory("TargetPosition", fishingSpot);
	}
}