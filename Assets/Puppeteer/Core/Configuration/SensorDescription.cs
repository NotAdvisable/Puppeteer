using Puppeteer.Core.Debug;
using System;
using System.Xml.Serialization;

namespace Puppeteer.Core.Configuration
{
	public class SensorDescription : BasicDescription
	{
		public string ExecuteableSensorTypeName
		{
			get
			{
				return ExecutableSensorType.AssemblyQualifiedName;
			}
			set
			{
				ExecutableSensorType = Type.GetType(value);
#if UNITY_EDITOR
				if (ExecutableSensorType == null)
				{
					string fullName = value.Substring(value.IndexOf(","));
					foreach (var entry in UnityEditor.TypeCache.GetTypesDerivedFrom<PuppeteerExecutableSensor>())
					{
						if (entry.FullName == fullName)
						{
							ExecutableSensorType = entry;
						}
					}
				}
#endif
				if (ExecutableSensorType == null)
				{
					ExecutableSensorType = typeof(DefaultSensor);
					PuppeteerLogger.Log(string.Format("SensorDescription couldn't be loaded. Type [{0}] wasn't found. " +
													"Sensor will be set to Puppeteer.DefaultSensor and will therefore do nothing.", value),
													LogType.Error);
				}
			}
		}

		[XmlIgnore]
		public Type ExecutableSensorType;

		public bool ShouldBeTicked;
		public int ExecutionOrder;
		public string ManagedWorldState;
		public int TickRate;
	}
}