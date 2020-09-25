using System.Collections.Generic;

namespace Puppeteer.Core.Action
{
	public class ActionSet<TKey, TValue> : HashSet<IAction<TKey, TValue>>
	{
	}
}