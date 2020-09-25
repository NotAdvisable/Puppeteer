using Puppeteer.Core.Utility;
using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI
{
	internal static class PuppeteerEditorGUIHelper
	{
		static PuppeteerEditorGUIHelper()
		{
			ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#191919" : "#8a8a8a", out var colour);
			DIVIDER_COLOUR = colour;
		}

		public static void DrawGraphInspectorHeader(Texture _icon, string _typeText, string _headerText)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Box(_icon);
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(_typeText);
			EditorGUILayout.LabelField(_headerText, INSPECTOR_HEADER_LABEL_STYLE);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		public static void DrawDivider()
		{
			var rect = GUILayoutUtility.GetRect(1f, 1f);

			rect.xMin = 0f;
			rect.width += 4f;

			if (Event.current.type != EventType.Repaint)
				return;

			EditorGUI.DrawRect(rect, DIVIDER_COLOUR);
		}

		public static void DrawHorizontal(params string[] _entries)
		{
			EditorGUILayout.BeginHorizontal();
			for (int i = 0; i < _entries.Length;)
			{
				EditorGUILayout.LabelField(_entries[i]);
				if(++i < _entries.Length)
				{
					GUILayout.Space(0);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private static readonly GUIStyle INSPECTOR_HEADER_LABEL_STYLE = new GUIStyle()
		{
			normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black },
			fontSize = 16,
		};

		private static readonly Color DIVIDER_COLOUR;
	}
}