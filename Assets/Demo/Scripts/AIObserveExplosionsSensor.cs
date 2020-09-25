using Puppeteer.Core;
using UnityEngine;

public class AIObserveExplosionsSensor : PuppeteerExecutableSensor
{
	public override bool DetectWorldStateChange(PuppeteerAgent _owner)
	{
		var lastExplosionPosition = ExplosionManager.Instance.GetLastExplosionPositionIfAny();
		if (!lastExplosionPosition.HasValue)
		{
			return false;
		}

		float range = ExplosionManager.Instance.GetDangerRange();
		float squaredDistance = Vector3.SqrMagnitude(lastExplosionPosition.Value - _owner.gameObject.transform.position);
		if (squaredDistance > range * range)
		{
			return false;
		}

		_owner.AddOrUpdateWorkingMemory("LastInRangeExplosionPosition", lastExplosionPosition.Value);
		_owner.AddOrUpdateWorkingMemory(m_ManagedWorldState, true);
		return true;
	}

	public override void Initialise(PuppeteerAgent _owner)
	{
		var sensorDesc = PuppeteerManager.Instance.GetSensorDescription(DescriptionGUID);
		if (sensorDesc != null)
		{
			m_ManagedWorldState = sensorDesc.ManagedWorldState;
		}
	}

	private string m_ManagedWorldState;
}