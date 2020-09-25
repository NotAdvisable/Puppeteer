using Puppeteer.Core.Action;
using System.Collections.Generic;

namespace Puppeteer.Core
{
	public class Plan<TKey, TValue> : Stack<IAction<TKey, TValue>>
	{
		public bool Empty()
		{
			return Count == 0;
		}

		public bool IsValid(Agent<TKey, TValue> _executingAgent)
		{
			if (Empty())
			{
				return false;
			}

			foreach (IAction<TKey, TValue> action in this)
			{
				if (!action.IsValid(_executingAgent))
				{
					return false;
				}
			}

			return true;
		}
	}
}