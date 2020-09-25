using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;
using UnityEngine.AI;

public class AIHideAction : PuppeteerExecutableAction
{
	public override void Cancel()
	{
		base.Cancel();
		m_Animator.SetBool(ANIMATOR_IS_DUCKING_KEY, false);
		if (m_Agent.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
		{
			navMeshAgent.speed = 7;
		}
	}

	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		if (!IsValid(_executingAgent))
		{
			return;
		}

		if(_executingAgent.GetWorkingMemory().TryGetValue("LastInRangeExplosionPosition", out var value))
		{
			m_HidingFromExplosionPosition = (Vector3)value;
		}

		if (_executingAgent.TryGetComponent<Animator>(out var animator))
		{
			m_Animator = animator;

			animator.SetTrigger(ANIMATOR_START_DUCKING_KEY);
			animator.SetBool(ANIMATOR_IS_DUCKING_KEY, true);
		}

		m_EndHideAt = ExplosionManager.Instance.GetRandomHideDuration() + Time.time;
	}

	public override void Execute()
	{
		if (m_Agent.GetWorkingMemory().TryGetValue("LastInRangeExplosionPosition", out var value))
		{
			if(Vector3.SqrMagnitude(m_HidingFromExplosionPosition - (Vector3)value) > 1)
			{
				m_CurrentActionState = ActionState.Failed;
				return;
			}
		}

		if (Time.time < m_EndHideAt)
		{
			return;
		}

		m_CurrentActionState = ActionState.Completed;
	}

	public override void Exit()
	{
		base.Exit();
		
		m_Animator.SetBool(ANIMATOR_IS_DUCKING_KEY, false);
		if (m_Agent.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
		{
			navMeshAgent.speed = 7;
		}
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

	private static readonly string ANIMATOR_IS_DUCKING_KEY = "IsDucking";
	private static readonly string ANIMATOR_START_DUCKING_KEY = "StartDucking";

	private Vector3 m_HidingFromExplosionPosition;

	private Animator m_Animator;
	private float m_EndHideAt;
}