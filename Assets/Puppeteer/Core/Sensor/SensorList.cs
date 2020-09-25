using System.Collections.Generic;

namespace Puppeteer.Core.Sensor
{
	public class SensorList<TKey, TValue> : List<ISensor<TKey, TValue>>
	{
		private static int CompareSensorOrder(ISensor<TKey, TValue> _lhs, ISensor<TKey, TValue> _rhs)
		{
			return _lhs.GetOrder().CompareTo(_rhs.GetOrder());
		}

		public void SortByOrder()
		{
			Sort(CompareSensorOrder);
		}

		public void AddAndSort(ISensor<TKey, TValue> _item)
		{
			Add(_item);
			Sort(CompareSensorOrder);
		}
	}
}