using Puppeteer.Core;
using Puppeteer.Core.Action;
using System.Collections.Generic;
using UnityEngine;

public class AIObtainFishFromRiverAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		if (!IsValid(_executingAgent))
		{
			return;
		}

		var animator = _executingAgent.GetComponent<Animator>();
		animator.SetTrigger(ANIMATOR_START_FISHING_KEY);

		m_EndFishingAt = FishManager.Instance.GetRandomFishingDuration() + Time.time;
	}

	public override void Execute()
	{
		if (Time.time < m_EndFishingAt)
		{
			return;
		}

		m_CurrentActionState = ActionState.Completed;

		var fishSpawnPosition = m_Agent.transform.position - m_Agent.transform.forward;
		fishSpawnPosition.y += 4;
		FishManager.Instance.SpawnFish(fishSpawnPosition);
	}

	public override void Exit()
	{
		foreach (KeyValuePair<string, object> effect in m_Effects)
		{
			if (effect.Key != OBTAIN_FISH_FROM_RIVER_KEY)
			{
				m_Agent.AddOrUpdateWorkingMemory(effect.Key, effect.Value);
			}
		}

		base.Exit(false);
	}

	public override bool IsValid(PuppeteerAgent _executingAgent)
	{
		if (_executingAgent.TryGetComponent<Animator>(out _))
		{
			return true;
		}
		
		m_CurrentActionState = ActionState.Failed;
		return false;
	}

	private static readonly string ANIMATOR_START_FISHING_KEY = "StartFishing";
	private static readonly string OBTAIN_FISH_FROM_RIVER_KEY = "ObtainFishFromRiver";
	private float m_EndFishingAt;
}