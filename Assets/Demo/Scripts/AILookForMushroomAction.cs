using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;

public class AILookForMushroomAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		GameObject closestMushroom = MushroomManager.Instance.GetClosestUnclaimedMushroomToPosition(_executingAgent.gameObject.transform.position);

		if (closestMushroom == null)
		{
			m_CurrentActionState = ActionState.Failed;
			return;
		}

		_executingAgent.AddOrUpdateWorkingMemory("TargetPosition", closestMushroom.transform.position);
		_executingAgent.AddOrUpdateWorkingMemory("PickupTarget", closestMushroom);
	}
}