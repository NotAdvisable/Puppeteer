using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;

public class AILookForFishAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		GameObject closestFish = FishManager.Instance.GetClosestUnclaimedFishToPosition(_executingAgent.gameObject.transform.position);

		if (closestFish == null)
		{
			m_CurrentActionState = ActionState.Failed;
			return;
		}

		_executingAgent.AddOrUpdateWorkingMemory("TargetPosition", closestFish.transform.position);
		_executingAgent.AddOrUpdateWorkingMemory("PickupTarget", closestFish);
	}
}