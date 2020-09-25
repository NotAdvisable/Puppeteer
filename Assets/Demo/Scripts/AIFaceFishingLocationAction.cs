using Puppeteer.Core;
using Puppeteer.Core.Action;
using UnityEngine;

public class AIFaceFishingLocationAction : PuppeteerExecutableAction
{
	public override void Enter(PuppeteerAgent _executingAgent)
	{
		base.Enter(_executingAgent);

		if (!IsValid(_executingAgent))
		{
			return;
		}

		m_FishingLocation = FishManager.Instance.GetCurrentFishingSpotPosition();
	}

	public override void Execute()
	{
		Vector3 dir = m_FishingLocation - m_Agent.transform.position;
		dir.y = 0;
		Quaternion rot = Quaternion.LookRotation(dir);
		m_Agent.transform.rotation = Quaternion.Lerp(m_Agent.transform.rotation, rot, 3 * Time.deltaTime);
		if (Mathf.Abs(Quaternion.Dot(rot, m_Agent.transform.rotation)) > 0.99f)
		{
			m_CurrentActionState = ActionState.Completed;
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

	private Vector3 m_FishingLocation;
}