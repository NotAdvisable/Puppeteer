using Puppeteer.Core.Debug;
using System;
using System.Xml.Serialization;

namespace Puppeteer.Core.Configuration
{
	public class ActionDescription : BasicDescription
	{
		public WorldStateDescription[] Preconditions;
		public WorldStateDescription[] Effects;
		public float BaseUtility;

		public string ExecuteableActionTypeName
		{
			get
			{
				return ExecutableActionType.AssemblyQualifiedName;
			}
			set
			{
				ExecutableActionType = Type.GetType(value);
#if UNITY_EDITOR
				if (ExecutableActionType == null)
				{
					string fullName = value.Substring(value.IndexOf(","));
					foreach (var entry in UnityEditor.TypeCache.GetTypesDerivedFrom<PuppeteerExecutableAction>())
					{
						if (entry.FullName == fullName)
						{
							ExecutableActionType = entry;
						}
					}
				}
#endif
				if (ExecutableActionType == null)
				{
					ExecutableActionType = typeof(DefaultAction);
					PuppeteerLogger.Log(string.Format("ActionDescription couldn't be loaded. Type [{0}] wasn't found. " +
													"Action will be set to Puppeteer.DefaultAction and will therefore do nothing.", 
													value), 
										LogType.Error);
				}
			}
		}

		[XmlIgnore]
		public Type ExecutableActionType;
	}
}