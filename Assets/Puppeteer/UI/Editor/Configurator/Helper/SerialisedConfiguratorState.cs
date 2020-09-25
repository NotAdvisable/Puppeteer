using UnityEngine;

namespace Puppeteer.UI
{
	public class SerialisedConfiguratorState : ScriptableObject
	{
		public SerialisableGuid LastOpenedAction;
		public int LastOpenedAgentObjectID;
		public SerialisableGuid LastOpenedArchetype;
		public SerialisableGuid LastOpenedGoal;
		public SerialisableGuid LastOpenedSensor;
		public string TabTypeName;
	}
}