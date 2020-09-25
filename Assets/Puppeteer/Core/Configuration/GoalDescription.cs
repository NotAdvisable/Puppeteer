namespace Puppeteer.Core.Configuration
{
	public class GoalDescription : BasicDescription
	{
		public WorldStateDescription[] GoalParts;
		public float BaseUtility;
		public UtilityDescription[] UtilityParts;
	}
}