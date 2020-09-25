namespace Puppeteer.Core.Debug
{
	public enum LogType
	{
		Assertion,
		Error,
		Warning,
		Log
	}

	public static class PuppeteerLogger
	{
		public static void Log(object _message, LogType _logLevel = LogType.Log, UnityEngine.Object _context = null)
		{
			_message = PUPPETEER_TAG + _message;

			switch (_logLevel)
			{
				case LogType.Assertion:
					if (_context is null)
					{
						UnityEngine.Debug.LogAssertion(_message);
					}
					else
					{
						UnityEngine.Debug.LogAssertion(_message, _context);
					}
					break;

				case LogType.Error:
					if (_context is null)
					{
						UnityEngine.Debug.LogError(_message);
					}
					else
					{
						UnityEngine.Debug.LogError(_message, _context);
					}
					break;

				case LogType.Warning:
					if (_context is null)
					{
						UnityEngine.Debug.LogWarning(_message);
					}
					else
					{
						UnityEngine.Debug.LogWarning(_message, _context);
					}
					break;

				case LogType.Log:
				default:
					if (_context is null)
					{
						UnityEngine.Debug.Log(_message);
					}
					else
					{
						UnityEngine.Debug.Log(_message, _context);
					}
					break;
			}
		}

		private static readonly string PUPPETEER_TAG = "[Puppeteer AI] ";
	}
}