using Puppeteer.Core.Planning;

namespace Puppeteer.Core.Sensor
{
	public interface ISensor<TKey, TValue>
	{
		bool DetectWorldStateChange(IAgent<TKey, TValue> _owner);

		int GetOrder();

		bool GetShouldBeTicked();

		int GetTickRate();

		void Initialise(Agent<TKey, TValue> _owner);

		void SetOrder(int _order);

		void SetShouldBeTicked(bool _value);

		void SetTickRate(int _tickRate);
	}
}