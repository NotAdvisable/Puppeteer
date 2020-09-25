using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;
using UnityEngine.AI;

public class AINavmeshMoveToAction : PuppeteerExecutableAction
{
	public override void Cancel()
	{
		base.Cancel();

		m_Animator.SetFloat(ANIMATOR_MOVE_SPEED_KEY, 0);
		m_NavMeshAgent.isStopped = true;
	}

	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		if (!IsValid(_executingAgent))
		{
			m_CurrentActionState = ActionState.Failed;
			return;
		}

		m_TargetPosition = (Vector3)_executingAgent.GetWorkingMemory()[TARGET_POSITION_KEY];
		m_Animator = _executingAgent.GetComponent<Animator>();
		m_NavMeshAgent = _executingAgent.GetComponent<NavMeshAgent>();
		m_NavMeshAgent.isStopped = false;
		m_NavMeshAgent.SetDestination(m_TargetPosition);
	}

	public override void Execute()
	{
		if (CheckReachedDestination())
		{
			m_CurrentActionState = ActionState.Completed;
			m_Animator.SetFloat(ANIMATOR_MOVE_SPEED_KEY, 0);
			return;
		}

		m_Animator.SetFloat(ANIMATOR_MOVE_SPEED_KEY, m_NavMeshAgent.velocity.magnitude);
	}


	private bool CheckReachedDestination()
	{
		// Check if we've reached the destination
		if (!m_NavMeshAgent.pathPending)
		{
			if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
			{
				if (!m_NavMeshAgent.hasPath || m_NavMeshAgent.velocity.sqrMagnitude == 0f)
				{
					return true;
				}
			}
		}

		return false;
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override bool IsValid(PuppeteerAgent _executingAgent)
	{
		if (_executingAgent.gameObject.GetComponent<NavMeshAgent>() == null)
		{
			return false;
		}

		return _executingAgent.GetWorkingMemory().ContainsKey(TARGET_POSITION_KEY);
	}

	private static readonly string ANIMATOR_MOVE_SPEED_KEY = "MoveSpeed";
	private static readonly string TARGET_POSITION_KEY = "TargetPosition";
	private Animator m_Animator;
	private NavMeshAgent m_NavMeshAgent;
	private Vector3 m_TargetPosition;
}