using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;
using UnityEngine.AI;

public class AIFindSafetyAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		if(_executingAgent.GetWorkingMemory().TryGetValue("LastInRangeExplosionPosition", out var lastPosition))
		{
			var fleeDirection = (m_Agent.transform.position - (Vector3)lastPosition).normalized;
			_executingAgent.AddOrUpdateWorkingMemory("TargetPosition", m_Agent.transform.position + fleeDirection * ExplosionManager.Instance.GetDangerRange() * 2);

			if (_executingAgent.TryGetComponent<Animator>(out var animator))
			{
				animator.SetTrigger(ANIMATOR_START_INJURED_RUN_KEY);

				if (_executingAgent.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
				{
					navMeshAgent.speed = 3;
				}
			}
		}
		else
		{
			m_CurrentActionState = ActionState.Failed;
			return;
		}

	}

	public override void Cancel()
	{
		base.Cancel();
		if (m_Agent.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
		{
			navMeshAgent.speed = 7;
		}
	}

	private static readonly string ANIMATOR_START_INJURED_RUN_KEY = "StartInjuredRun";

}