namespace Puppeteer.Core.Helper
{
	public class SingletonManager<T> where T : class, new()
	{
		public static T Instance
		{
			get
			{
				lock (INSTANCE_LOCK)
				{
					if (m_InstanceValue != null)
					{
						return m_InstanceValue;
					}

					if (m_InstanceValue == null)
					{
						m_InstanceValue = new T();
					}

					return m_InstanceValue;
				}
			}
			set
			{
				m_InstanceValue = value;
			}
		}

		private static readonly object INSTANCE_LOCK = new object();
		private static T m_InstanceValue = null;
	}
}