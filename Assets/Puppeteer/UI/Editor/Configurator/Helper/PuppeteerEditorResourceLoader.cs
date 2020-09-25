using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI
{
	internal static class PuppeteerEditorResourceLoader
	{
		public static Sprite ActionIcon32
		{
			get
			{
				if (m_ActionIcon32 == null)
				{
					m_ActionIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_action" : "Images/32px/icon_action_black");
				}
				return m_ActionIcon32;
			}
		}

		public static Sprite AgentIcon32
		{
			get
			{
				if (m_AgentIcon32 == null)
				{
					m_AgentIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_agent" : "Images/32px/icon_agent_black");
				}
				return m_AgentIcon32;
			}
		}

		public static Sprite ArchetypeIcon32
		{
			get
			{
				if (m_ArchetypeIcon32 == null)
				{
					m_ArchetypeIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_archetype_02" : "Images/32px/icon_archetype_02_black");
				}
				return m_ArchetypeIcon32;
			}
		}

		public static Sprite ConfiguratorIcon16
		{
			get
			{
				if (m_ConfiguratorIcon16 == null)
				{
					m_ConfiguratorIcon16 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/16px/icon_gearwheel_02" : "Images/16px/icon_gearwheel_02_black");
				}
				return m_ConfiguratorIcon16;
			}
		}

		public static Sprite ConfiguratorIcon32
		{
			get
			{
				if (m_ConfiguratorIcon32 == null)
				{
					m_ConfiguratorIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_gearwheel_02" : "Images/32px/icon_gearwheel_02_black");
				}
				return m_ConfiguratorIcon32;
			}
		}

		public static Sprite GoalIcon16
		{
			get
			{
				if (m_GoalIcon16 == null)
				{
					m_GoalIcon16 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/16px/icon_goal_02" : "Images/16px/icon_goal_02_black");
				}
				return m_GoalIcon16;
			}
		}

		public static Sprite GoalIcon32
		{
			get
			{
				if (m_GoalIcon32 == null)
				{
					m_GoalIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_goal_02" : "Images/32px/icon_goal_02_black");
				}
				return m_GoalIcon32;
			}
		}

		public static Sprite LogoIcon16
		{
			get
			{
				if (m_LogoIcon16 == null)
				{
					m_LogoIcon16 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/16px/icon_logo_03" : "Images/16px/icon_logo_03_black");
				}
				return m_LogoIcon16;
			}
		}

		public static Sprite LogoIcon32
		{
			get
			{
				if (m_LogoIcon32 == null)
				{
					m_LogoIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_logo_03" : "Images/32px/icon_logo_03_black");
				}
				return m_LogoIcon32;
			}
		}

		public static Sprite PlanActiveActionIcon64
		{
			get
			{
				if (m_PlanActiveActionIcon64 == null)
				{
					m_PlanActiveActionIcon64 = Resources.Load<Sprite>("Images/active_action");
				}
				return m_PlanActiveActionIcon64;
			}
		}

		public static Sprite SensorIcon32
		{
			get
			{
				if (m_SensorIcon32 == null)
				{
					m_SensorIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_sensor" : "Images/32px/icon_sensor_black");
				}
				return m_SensorIcon32;
			}
		}

		public static Sprite TextLogo32
		{
			get
			{
				if (m_TextLogo32 == null)
				{
					m_TextLogo32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/textLogo32" : "Images/textLogo32_black");
				}
				return m_TextLogo32;
			}
		}

		public static Sprite VisualiserIcon16
		{
			get
			{
				if (m_VisualiserIcon16 == null)
				{
					m_VisualiserIcon16 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/16px/icon_node_system" : "Images/16px/icon_node_system_black");
				}
				return m_VisualiserIcon16;
			}
		}

		public static Sprite VisualiserIcon32
		{
			get
			{
				if (m_VisualiserIcon32 == null)
				{
					m_VisualiserIcon32 = Resources.Load<Sprite>(EditorGUIUtility.isProSkin ? "Images/32px/icon_node_system_02" : "Images/32px/icon_node_system_02_black");
				}
				return m_VisualiserIcon32;
			}
		}

		private static Sprite m_ActionIcon32 = null;
		private static Sprite m_AgentIcon32 = null;
		private static Sprite m_ArchetypeIcon32 = null;
		private static Sprite m_ConfiguratorIcon16 = null;
		private static Sprite m_ConfiguratorIcon32 = null;
		private static Sprite m_GoalIcon16 = null;
		private static Sprite m_GoalIcon32 = null;
		private static Sprite m_LogoIcon16 = null;
		private static Sprite m_LogoIcon32 = null;
		private static Sprite m_PlanActiveActionIcon64 = null;
		private static Sprite m_SensorIcon32 = null;
		private static Sprite m_TextLogo32 = null;
		private static Sprite m_VisualiserIcon16 = null;
		private static Sprite m_VisualiserIcon32 = null;
	}
}