using Puppeteer.Core.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Puppeteer.Core.WorldState
{
	public abstract class WorldStateBase<TKey, TValue> : IEquatable<WorldStateBase<TKey, TValue>>, IEnumerable
	{
		internal Dictionary<TKey, TValue> GetKeyValuePairs()
		{
			return m_WorldState;
		}

		public override string ToString()
		{
			return "{" + m_WorldState.ToString(": ", "} , {") + "}";
		}

		public IEnumerator GetEnumerator()
		{
			return m_WorldState.GetEnumerator();
		}

		public void Add(TKey _key, TValue _value)
		{
			m_WorldState.Add(_key, _value);
		}

		public override int GetHashCode()
		{
			return m_WorldState.GetHashCode();
		}

		public bool Equals(WorldStateBase<TKey, TValue> _other)
		{
			if (_other is null)
			{
				return false;
			}

			if (m_WorldState.Count != _other.m_WorldState.Count)
			{
				return false;
			}

			return m_WorldState.All(_entry =>
			{
				if (_other.m_WorldState.TryGetValue(_entry.Key, out var otherValue))
				{
					return otherValue.Equals(_entry.Value);
				}
				else
				{
					return false;
				}
			});
		}

		protected Dictionary<TKey, TValue> m_WorldState = new Dictionary<TKey, TValue>();
	}
}