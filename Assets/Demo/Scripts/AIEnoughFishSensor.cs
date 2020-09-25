using Puppeteer.Core;

public class AIEnoughFishSensor : PuppeteerExecutableSensor
{
	public override void Initialise(PuppeteerAgent _owner)
	{
		m_Agent = _owner;

		FishManager.Instance.OnFishCountChanged += SetHasEnoughFish;

		var sensorDesc = PuppeteerManager.Instance.GetSensorDescription(DescriptionGUID);
		m_ManagedWorldState = sensorDesc.ManagedWorldState;
	}

	private void SetHasEnoughFish(int _desiredCount, int _hasCount)
	{
		m_Agent.AddOrUpdateWorkingMemory(m_ManagedWorldState, _hasCount >= _desiredCount);
	}


	private PuppeteerAgent m_Agent;
	private string m_ManagedWorldState;

}