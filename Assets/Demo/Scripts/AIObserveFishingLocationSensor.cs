using Puppeteer.Core;
using UnityEngine;

public class AIObserveFishingLocationSensor : PuppeteerExecutableSensor
{
	public override bool DetectWorldStateChange(PuppeteerAgent _owner)
	{
		m_Agent.GetWorkingMemory().TryGetValue(m_ManagedWorldState, out var value);

		var fishingSpot = FishManager.Instance.GetCurrentFishingSpotPosition();

		if (value == null)
		{
			m_Agent.AddOrUpdateWorkingMemory(m_ManagedWorldState, fishingSpot);
			m_Agent.AddOrUpdateWorkingMemory(AT_TARGET_POSITION_KEY, false);
			m_Agent.AddOrUpdateWorkingMemory(HAS_TARGET_POSITION_KEY, true);
			return true;
		}

		if (Vector3.Distance((Vector3)value, fishingSpot) > 1.0f)
		{
			m_Agent.AddOrUpdateWorkingMemory(m_ManagedWorldState, fishingSpot);
			m_Agent.AddOrUpdateWorkingMemory(AT_TARGET_POSITION_KEY, false);
			m_Agent.AddOrUpdateWorkingMemory(HAS_TARGET_POSITION_KEY, true);
			return true;
		}

		return false;
	}

	public override void Initialise(PuppeteerAgent _owner)
	{
		m_Agent = _owner;

		var sensorDesc = PuppeteerManager.Instance.GetSensorDescription(DescriptionGUID);
		if (sensorDesc != null)
		{
			m_ManagedWorldState = sensorDesc.ManagedWorldState;
		}
	}


	private string m_ManagedWorldState; 
	private static readonly string AT_TARGET_POSITION_KEY = "AtTargetPosition";
	private static readonly string HAS_TARGET_POSITION_KEY = "HasTargetPosition";
	private PuppeteerAgent m_Agent;
}