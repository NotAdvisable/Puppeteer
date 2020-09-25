using Puppeteer.UI.External.GraphVisualizer;
using System;
using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI
{
	internal class PuppeteerPlanGraphRenderer : DefaultGraphRenderer
	{
		public PuppeteerPlanGraphRenderer(Action<VisualiserBaseNode> _nodeClickedCallback)
		{
			OnNodeClicked += _node => _nodeClickedCallback(_node as VisualiserBaseNode);

			ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#282828" : "#a5a5a5", out var colour);
			m_InspectorBackgroundColour = colour;
		}

		public void Draw(IGraphLayout _graphLayout, Rect _totalDrawingArea, GraphSettings _graphSettings, PuppeteerPlanVisualiser _visualiser)
		{
			var drawingArea = new Rect(_totalDrawingArea);

			if (_graphSettings.ShowInspector)
			{
				var inspectorArea = new Rect(_totalDrawingArea)
				{
					width = Mathf.Max(INSPECTOR_FIXED_WIDTH, drawingArea.width * 0.25f) + BORDER_SIZE * 2
				};

				inspectorArea.x = drawingArea.xMax - inspectorArea.width;
				drawingArea.width -= inspectorArea.width;

				DrawInspector(inspectorArea, _visualiser);
			}

			Draw(_graphLayout, drawingArea, _graphSettings);
		}

		private void DrawInspector(Rect _inspectorArea, PuppeteerPlanVisualiser _visualiser)
		{
			EditorGUI.DrawRect(_inspectorArea, m_InspectorBackgroundColour);

			_inspectorArea.Apply(ref m_BorderOffsetRect);

			GUILayout.BeginArea(_inspectorArea);
			GUILayout.BeginVertical();

			if (m_SelectedNode != null)
			{
				using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
				{
					m_ScrollPosition = scrollView.scrollPosition;
				}

				(m_SelectedNode as VisualiserBaseNode)?.DrawInspector(_visualiser);
			}
			else
			{
				GUILayout.Label("Click on a node\nto display its details.");
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private static readonly float BORDER_SIZE = 15;
		private static readonly float INSPECTOR_FIXED_WIDTH = 100;
		
		private static Rect m_BorderOffsetRect = new Rect(BORDER_SIZE, BORDER_SIZE, -(BORDER_SIZE * 2), - (BORDER_SIZE * 2));
		
		private readonly Color m_InspectorBackgroundColour;
		
		private Vector2 m_ScrollPosition;
	}
}