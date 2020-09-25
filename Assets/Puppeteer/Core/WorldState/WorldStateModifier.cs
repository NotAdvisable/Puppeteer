using System.Collections.Generic;
using System.Linq;

namespace Puppeteer.Core.WorldState
{
	public class WorldStateModifier<TKey, TValue> : WorldStateBase<TKey, TValue>
	{
		internal bool ConflictsAny(WorldStateModifier<TKey, TValue> _other)
		{
			if (_other == null)
			{
				return false;
			}

			return ConflictsAny(_other.m_WorldState);
		}

		internal bool ConflictsAny(WorldState<TKey, TValue> _worldState)
		{
			if (_worldState == null)
			{
				return false;
			}

			return ConflictsAny(_worldState.GetKeyValuePairs());
		}

		internal bool ContainsAny(WorldState<TKey, TValue> _worldState)
		{
			if (_worldState == null)
			{
				return false;
			}

			return _worldState.GetKeyValuePairs().Any((_otherPair) =>
													{ return m_WorldState.TryGetValue(_otherPair.Key, out TValue ownValue) && _otherPair.Value.Equals(ownValue); });
		}

		private bool ConflictsAny(Dictionary<TKey, TValue> _otherPairs)
		{
			return _otherPairs.Any((_otherPair) =>
								{ return m_WorldState.TryGetValue(_otherPair.Key, out TValue ownValue) && !_otherPair.Value.Equals(ownValue); });
		}
	}
}