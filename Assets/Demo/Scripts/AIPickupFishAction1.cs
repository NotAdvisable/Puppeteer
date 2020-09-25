using Puppeteer.Core;
using UnityEngine;

public class AIPickupFishAction1 : AIPickupFishAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		var targetFish = (GameObject)_executingAgent.GetWorkingMemory()["PickupTarget"];
		if (targetFish != null)
		{
			if (_executingAgent.gameObject.TryGetComponent<CarryHelper>(out var carryHelper))
			{
				carryHelper.ReplaceCarriedObject(targetFish);
			}
		}

		_executingAgent.AddOrUpdateWorkingMemory("TargetPosition", FishManager.Instance.GetHomeBaseTransform().position);
	}
}