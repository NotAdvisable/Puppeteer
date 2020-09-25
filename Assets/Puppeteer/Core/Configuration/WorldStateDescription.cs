namespace Puppeteer.Core.Configuration
{
	public class WorldStateDescription
	{
		public WorldStateDescription()
		{
		}

		public WorldStateDescription(WorldStateDescription _worldStateDescription)
		{
			Key = _worldStateDescription.Key;
			Value = _worldStateDescription.Value;
		}

		public string Key = string.Empty;
		public object Value = null;
	}
}