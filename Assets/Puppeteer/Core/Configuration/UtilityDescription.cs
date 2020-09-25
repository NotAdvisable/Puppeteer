using Puppeteer.Core.Utility;
using UnityEngine;

namespace Puppeteer.Core.Configuration
{
	public class UtilityDescription
	{
		public UtilityDescription()
		{
		}

		public UtilityDescription(UtilityDescription _utilityDescription)
		{
			UtilityOperator = _utilityDescription.UtilityOperator;
			UtilityCurve = _utilityDescription.UtilityCurve;
			WorldStateName = _utilityDescription.WorldStateName;
			CurveMultiplier = _utilityDescription.CurveMultiplier;
		}

		public UtilityOperators UtilityOperator;
		public AnimationCurve UtilityCurve;
		public string WorldStateName = string.Empty;
		public float CurveMultiplier;
	}
}