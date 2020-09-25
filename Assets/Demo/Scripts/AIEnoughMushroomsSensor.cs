using Puppeteer.Core;

public class AIEnoughMushroomsSensor : PuppeteerExecutableSensor
{
	public override void Initialise(PuppeteerAgent _owner)
	{
		m_Agent = _owner;
		MushroomManager.Instance.OnMushroomCountChanged += SetHasEnoughMushrooms;

		var sensorDesc = PuppeteerManager.Instance.GetSensorDescription(DescriptionGUID);
		m_ManagedWorldState = sensorDesc.ManagedWorldState;
	}

	private void SetHasEnoughMushrooms(int _desiredCount, int _hasCount)
	{
		m_Agent.AddOrUpdateWorkingMemory(m_ManagedWorldState, _hasCount >= _desiredCount);
		m_Agent.AddOrUpdateWorkingMemory(m_NeedForMushroomKey, 1.0f - (float)_hasCount / _desiredCount);
	}

	private const string m_NeedForMushroomKey = "NeedForMushrooms";
	
	private PuppeteerAgent m_Agent;
	private string m_ManagedWorldState;
}