using System.Collections.Generic;
using System.Linq;

namespace Puppeteer.Core.WorldState
{
	public class WorldState<TKey, TValue> : WorldStateBase<TKey, TValue>
	{
		public WorldState()
		{
		}

		public TValue this[TKey _key] => m_WorldState[_key];

		public static WorldState<TKey, TValue> operator -(WorldState<TKey, TValue> _lhs, WorldState<TKey, TValue> _rhs)
		{
			WorldState<TKey, TValue> difference = new WorldState<TKey, TValue>();
			if (_lhs == null || _lhs.m_WorldState == null)
			{
				if (_rhs != null && _rhs.m_WorldState != null)
				{
					foreach (var item in _rhs.m_WorldState)
					{
						difference.m_WorldState.Add(item.Key, item.Value);
					}
				}
			}
			else if (_rhs == null || _rhs.m_WorldState == null)
			{
				if (_lhs.m_WorldState != null)
				{
					foreach (var item in _lhs.m_WorldState)
					{
						difference.m_WorldState.Add(item.Key, item.Value);
					}
				}
			}
			else
			{
				foreach (var item in _lhs.m_WorldState)
				{
					if (_rhs.m_WorldState.TryGetValue(item.Key, out TValue otherValue))
					{
						if (otherValue.Equals(item.Value))
						{
							continue;
						}
					}

					difference.m_WorldState.Add(item.Key, item.Value);
				}
			}

			return difference;
		}

		public void AddOrUpdate(TKey _targetKey, TValue _value)
		{
			if (!UpdateEntryIfAvailable(_targetKey, _value))
			{
				m_WorldState.Add(_targetKey, _value);
			}
		}

		public void Apply(WorldStateModifier<TKey, TValue> _worldStateModifier)
		{
			if (_worldStateModifier == null)
			{
				return;
			}

			foreach (KeyValuePair<TKey, TValue> pair in _worldStateModifier.GetKeyValuePairs())
			{
				m_WorldState[pair.Key] = pair.Value;
			}
		}

		public void Clear()
		{
			m_WorldState.Clear();
		}

		public bool ContainsKey(TKey _key)
		{
			return m_WorldState.ContainsKey(_key);
		}

		public bool Remove(TKey _key)
		{
			return m_WorldState.Remove(_key);
		}

		public bool TryGetValue(TKey _key, out TValue _value)
		{
			return m_WorldState.TryGetValue(_key, out _value);
		}

		public bool UpdateEntryIfAvailable(TKey _targetKey, TValue _value)
		{
			if (!m_WorldState.ContainsKey(_targetKey))
			{
				return false;
			}

			m_WorldState[_targetKey] = _value;
			return true;
		}

		internal bool ContainsAll(WorldState<TKey, TValue> _worldState)
		{
			if (_worldState == null)
			{
				return false;
			}

			return _worldState.GetKeyValuePairs().All((_otherPair) =>
			{ return m_WorldState.TryGetValue(_otherPair.Key, out TValue ownValue) && _otherPair.Value.Equals(ownValue); });
		}

		public static T Copy<T>(T _worldState) where T : WorldState<TKey, TValue>, new()
		{
			return new T { m_WorldState = new Dictionary<TKey, TValue>(_worldState.m_WorldState)};
		}
	}
}