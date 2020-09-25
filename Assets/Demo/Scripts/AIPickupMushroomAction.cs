using Puppeteer.Core;
using UnityEngine;

public class AIPickupMushroomAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		var targetMushroom = (GameObject)_executingAgent.GetWorkingMemory()["PickupTarget"];
		if (targetMushroom != null)
		{
			if (_executingAgent.gameObject.TryGetComponent<CarryHelper>(out var carryHelper))
			{
				carryHelper.ReplaceCarriedObject(targetMushroom);
			}
		}

		_executingAgent.AddOrUpdateWorkingMemory("TargetPosition", MushroomManager.Instance.GetHomeBaseTransform().position);
	}
}