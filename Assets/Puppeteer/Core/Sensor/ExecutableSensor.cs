using Puppeteer.Core.Planning;

namespace Puppeteer.Core.Sensor
{
	public class ExecutableSensor<TKey, TValue> : ISensor<TKey, TValue>
	{
		public ExecutableSensor(int _order, int _frameTickRate)
		{
			m_Order = _order;
			m_TickRate = _frameTickRate;
		}

		protected ExecutableSensor()
		{
		}

		public virtual bool DetectWorldStateChange(IAgent<TKey, TValue> _owner)
		{
			return false;
		}

		public string GetManagedWorldState()
		{
			return m_ManagedWorldState;
		}

		public int GetOrder()
		{
			return m_Order;
		}

		public int GetTickRate()
		{
			return m_TickRate;
		}

		public virtual void Initialise(Agent<TKey, TValue> _owner)
		{
		}

		public void SetManagedWorldState(string _managedWorldState)
		{
			m_ManagedWorldState = _managedWorldState;
		}

		public void SetOrder(int _order)
		{
			m_Order = _order;
		}

		public void SetShouldBeTicked(bool _value)
		{
			m_ShouldBeTicked = _value;
		}

		public void SetTickRate(int _tickRate)
		{
			m_TickRate = _tickRate;
		}

		public bool GetShouldBeTicked()
		{
			return m_ShouldBeTicked;
		}

		private string m_ManagedWorldState;
		private int m_Order;
		private bool m_ShouldBeTicked;
		private int m_TickRate;
	}
}