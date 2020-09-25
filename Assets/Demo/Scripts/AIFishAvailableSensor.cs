using Puppeteer.Core;

public class AIFishAvailableSensor : PuppeteerExecutableSensor
{
	public override void Initialise(PuppeteerAgent _owner)
	{
		m_Agent = _owner;

		FishManager.Instance.OnFishSpawned += OnFishSpawnedOrClaimed;
		FishManager.Instance.OnFishClaimed += OnFishSpawnedOrClaimed;
	}

	private void OnFishSpawnedOrClaimed(int _spawnedCount, int _claimedCount)
	{
		var sensorDesc = PuppeteerManager.Instance.GetSensorDescription(DescriptionGUID);

		if (sensorDesc != null)
		{
			m_Agent.GetWorkingMemory().AddOrUpdate(sensorDesc.ManagedWorldState, (_spawnedCount - _claimedCount > 0));
			m_Agent.SetUnhandledSensorChanges(true);
		}
	}

	private PuppeteerAgent m_Agent;
}