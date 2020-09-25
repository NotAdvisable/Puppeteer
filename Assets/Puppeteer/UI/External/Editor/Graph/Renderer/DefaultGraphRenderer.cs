using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI.External.GraphVisualizer
{
	public class DefaultGraphRenderer : IGraphRenderer
	{
		public DefaultGraphRenderer()
		{
			ColorUtility.TryParseHtmlString("#2e86de", out var colour);
			m_SelectedNodeColour = colour;

			ColorUtility.TryParseHtmlString("#feca57", out colour);
			m_ActiveColour = colour;

			m_NodeRectStyle = new GUIStyle
			{
				normal =
				{
					background = (Texture2D)Resources.Load("Node"),
					textColor = Color.black,
				},
				border = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				wordWrap = true,
				clipping = TextClipping.Clip
			};

			m_NodeHeaderStyle = new GUIStyle
			{
				normal =
				{
					textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
				},

				border = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				wordWrap = true,
				clipping = TextClipping.Clip
			};

			m_NodeHeaderTextColour = EditorGUIUtility.isProSkin ? Color.white : Color.black;
		}

		public event Action<Node> OnNodeClicked;

		public void Draw(IGraphLayout _graphLayout, Rect _drawingArea)
		{
			GraphSettings defaults = new GraphSettings()
			{
				MaximumNormalizedNodeSize = DEFAULT_MAXIMUM_NORMALIZED_NODE_SIZE,
				MaximumNodeSizeInPixels = DEFAULT_MAXIMUM_NODE_SIZE_IN_PIXELS,
				NodeAspectRatio = DEFAULT_ASPECT_RATIO,
				ShowInspector = true,
			};

			Draw(_graphLayout, _drawingArea, defaults);
		}

		public void Draw(IGraphLayout _graphLayout, Rect _totalDrawingArea, GraphSettings _graphSettings)
		{
			if (m_SelectedNode != null)
			{
				Event currentEvent = Event.current;
				if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
				{
					Vector2 mousePos = currentEvent.mousePosition;
					if (_totalDrawingArea.Contains(mousePos))
					{
						m_SelectedNode = null;

						OnNodeClicked?.Invoke(m_SelectedNode);
					}
				}
			}

			DrawGraph(_graphLayout, _totalDrawingArea, _graphSettings);
		}

		public void DrawRect(Rect _rect, Color _colour, string _text, bool _isActive, bool _isSelected = false)
		{
			var originalColor = GUI.color;

			if (_isSelected)
			{
				GUI.color = m_SelectedNodeColour;
				float t = SELECTED_NODE_THICKNESS + (_isActive ? ACTIVE_NODE_THICKNESS : 0.0f);
				GUI.Box(new Rect(_rect.x - t, _rect.y - t, _rect.width + 2 * t, _rect.height + 2 * t),
					string.Empty, m_NodeRectStyle);
			}

			if (_isActive)
			{
				GUI.color = m_ActiveColour;
				GUI.Box(new Rect(_rect.x - ACTIVE_NODE_THICKNESS, _rect.y - ACTIVE_NODE_THICKNESS,
						_rect.width + 2 * ACTIVE_NODE_THICKNESS, _rect.height + 2 * ACTIVE_NODE_THICKNESS),
					string.Empty, m_NodeRectStyle);
			}

			// Body + Text
			GUI.color = _colour;
			m_NodeRectStyle.fontSize = ComputeFontSize(_rect.size, _text);
			GUI.Box(_rect, _text, m_NodeRectStyle);

			GUI.color = originalColor;
		}

		public Node GetSelectedNode()
		{
			return m_SelectedNode;
		}

		public void Reset()
		{
			m_SelectedNode = null;
		}

		private static int ComputeFontSize(Vector2 _nodeSize, string _text)
		{
			if (string.IsNullOrEmpty(_text))
			{
				return NODE_MAX_FONT_SIZE;
			}

			string[] words = _text.Split('\n');
			int numberOfLines = words.Length;
			int longestWord = words.Max(_entry => _entry.Length);

			// Approximate the text rectangle size using magic values.
			int width = longestWord * (int)(0.8f * NODE_MAX_FONT_SIZE);
			int height = numberOfLines * (int)(1.5f * NODE_MAX_FONT_SIZE);

			float factor = Math.Min(_nodeSize.x / width, _nodeSize.y / height);

			factor = Mathf.Clamp01(factor);

			return Mathf.CeilToInt(NODE_MAX_FONT_SIZE * factor);
		}

		// Apply node constraints to node size
		private static Vector2 ComputeNodeSize(Vector2 _scale, GraphSettings _graphSettings)
		{
			var extraTickness = (SELECTED_NODE_THICKNESS + ACTIVE_NODE_THICKNESS) * 2.0f;
			var nodeSize = new Vector2(_graphSettings.MaximumNormalizedNodeSize * _scale.x - extraTickness,
				_graphSettings.MaximumNormalizedNodeSize * _scale.y - extraTickness);

			// Adjust aspect ratio after scaling
			float currentAspectRatio = nodeSize.x / nodeSize.y;

			if (currentAspectRatio > _graphSettings.NodeAspectRatio)
			{
				// Shrink x dimension
				nodeSize.x = nodeSize.y * _graphSettings.NodeAspectRatio;
			}
			else
			{
				// Shrink y dimension
				nodeSize.y = nodeSize.x / _graphSettings.NodeAspectRatio;
			}

			// If node size is still too big, scale down
			if (nodeSize.x > _graphSettings.MaximumNodeSizeInPixels)
			{
				nodeSize *= _graphSettings.MaximumNodeSizeInPixels / nodeSize.x;
			}

			if (nodeSize.y > _graphSettings.MaximumNodeSizeInPixels)
			{
				nodeSize *= _graphSettings.MaximumNodeSizeInPixels / nodeSize.y;
			}

			return nodeSize;
		}

		private static void DrawEdge(Vector2 _from, Vector2 _to, float _weight)
		{
			GetTangents(_from, _to, out Vector3[] points, out Vector3[] tangents);

			Color color;
			if (Mathf.Approximately(_weight, float.MaxValue))
				color = m_ActiveColour;
			else
				color = Color.Lerp(EDGE_COLOR_MIN, EDGE_COLOR_MAX, _weight);

			Handles.DrawBezier(points[0], points[1], tangents[0], tangents[1], color, null, 5f);
		}

		// Compute the tangents for the graphLayout edges. Assumes that graphLayout is drawn from left to right
		private static void GetTangents(Vector2 _start, Vector2 _end, out Vector3[] _points, out Vector3[] _tangents)
		{
			_points = new Vector3[] { _start, _end };
			_tangents = new Vector3[2];

			// Heuristics to define the length of the tangents and tweak the look of the Bezier curves.
			const float minTangent = 30;
			const float weight = 0.5f;
			float cleverness = Mathf.Clamp01(((_start - _end).magnitude - 10) / 50);
			_tangents[0] = _start + new Vector2((_end.x - _start.x) * weight + minTangent, 0) * cleverness;
			_tangents[1] = _end + new Vector2((_end.x - _start.x) * -weight - minTangent, 0) * cleverness;
		}

		// Convert vertex position from normalized layout to render rect
		private static Vector2 ScaleVertex(Vector2 _vertex, Vector2 _offset, Vector2 _scaleFactor)
		{
			return new Vector2((_vertex.x + _offset.x) * _scaleFactor.x, (_vertex.y + _offset.y) * _scaleFactor.y);
		}

		// Draw the graph and returns the selected Node if there's any.
		private void DrawGraph(IGraphLayout _graphLayout, Rect _drawingArea, GraphSettings _graphSettings)
		{
			// add border, except on right-hand side where the legend will provide necessary padding
			_drawingArea = new Rect(_drawingArea.x + BORDER_SIZE,
				_drawingArea.y + BORDER_SIZE,
				_drawingArea.width - BORDER_SIZE * 2,
				_drawingArea.height - BORDER_SIZE * 2);

			var b = new Bounds(Vector3.zero, Vector3.zero);
			foreach (Vertex v in _graphLayout.Vertices)
			{
				b.Encapsulate(new Vector3(v.Position.x, v.Position.y, 0.0f));
			}

			// Increase b by maximum node size (since b is measured between node centers)
			b.Expand(new Vector3(_graphSettings.MaximumNormalizedNodeSize, _graphSettings.MaximumNormalizedNodeSize, 0));

			var scale = new Vector2(_drawingArea.width / b.size.x, _drawingArea.height / b.size.y);
			var offset = new Vector2(-b.min.x, -b.min.y);

			Vector2 nodeSize = ComputeNodeSize(scale, _graphSettings);

			GUI.BeginGroup(_drawingArea);

			foreach (var e in _graphLayout.Edges)
			{
				Vector2 v0 = ScaleVertex(e.Source.Position, offset, scale);
				Vector2 v1 = ScaleVertex(e.Destination.Position, offset, scale);
				Node node = e.Source.Node;

				if (_graphLayout.LeftToRight)
					DrawEdge(v1, v0, node.Weight);
				else
					DrawEdge(v0, v1, node.Weight);
			}

			Event currentEvent = Event.current;

			bool oldSelectionFound = false;
			Node newSelectedNode = null;

			foreach (Vertex v in _graphLayout.Vertices)
			{
				Vector2 nodeCenter = ScaleVertex(v.Position, offset, scale) - nodeSize / 2;
				var nodeRect = new Rect(nodeCenter.x, nodeCenter.y, nodeSize.x, nodeSize.y);

				bool clicked = false;
				if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
				{
					Vector2 mousePos = currentEvent.mousePosition;
					if (nodeRect.Contains(mousePos))
					{
						clicked = true;
						currentEvent.Use();
					}
				}

				bool currentSelection = (m_SelectedNode != null)
					&& v.Node.Content.Equals(m_SelectedNode.Content); // Make sure to use Equals() and not == to call any overriden comparison operator in the content type.

				DrawNode(nodeRect, v.Node, currentSelection || clicked);

				if (currentSelection)
				{
					// Previous selection still there.
					oldSelectionFound = true;
				}
				else if (clicked)
				{
					// Just Selected a new node.
					newSelectedNode = v.Node;
				}
			}

			if (newSelectedNode != null)
			{
				m_SelectedNode = newSelectedNode;

				OnNodeClicked?.Invoke(m_SelectedNode);
			}
			else if (!oldSelectionFound)
			{
				m_SelectedNode = null;
			}

			GUI.EndGroup();
		}

		// Draw a node an return true if it has been clicked
		private void DrawNode(Rect _nodeRect, Node _node, bool _isSelected)
		{
			string nodeType = _node.GetContentTypeName();
			string formattedLabel = Regex.Replace(nodeType, "((?<![A-Z])\\B[A-Z])", "\n$1"); // Split into multi-lines

			// nick: remove splits again if the following word has 3 or fewer characters (words like and, to, for, and at)
			formattedLabel = Regex.Replace(formattedLabel, "[\r\n](?=([A-z]{0,3}[\r\n]))", "");

			DrawRect(_nodeRect, _node.GetColor(), formattedLabel, _node.IsActive, _isSelected);

			DrawNodeHeaderTextIfNecessary(_node, ref _nodeRect);
		}

		private void DrawNodeHeaderText(ref Rect _nodeRect, string _text, Color _textColour)
		{
			Rect headerOffsetRect = new Rect(_nodeRect.x, _nodeRect.y - _nodeRect.height / 2, _nodeRect.width, _nodeRect.height / 2);

			m_NodeHeaderStyle.fontSize = ComputeFontSize(headerOffsetRect.size, _text);
			m_NodeHeaderStyle.normal.textColor = _textColour;

			GUI.Label(headerOffsetRect, _text, m_NodeHeaderStyle);
		}

		private void DrawNodeHeaderTextIfNecessary(Node _node, ref Rect _nodeRect)
		{
			if (_node.IsActive)
			{
				DrawNodeHeaderText(ref _nodeRect, "ACTIVE", EditorGUIUtility.isProSkin ? m_ActiveColour : Color.black);
			}
			else
			{
				switch (_node.TypeOfNode)
				{
					case Node.NodeType.GOAL:
						DrawNodeHeaderText(ref _nodeRect, "GOAL", m_NodeHeaderTextColour);
						break;

					case Node.NodeType.FINAL:
						DrawNodeHeaderText(ref _nodeRect, "START", m_NodeHeaderTextColour);
						break;

					default:
						break;
				}
			}
		}

		protected Node m_SelectedNode;

		private static readonly int ACTIVE_NODE_THICKNESS = 2;
		private static readonly float BORDER_SIZE = 15;
		private static readonly float DEFAULT_ASPECT_RATIO = 1.5f;
		private static readonly float DEFAULT_MAXIMUM_NODE_SIZE_IN_PIXELS = 100.0f;
		private static readonly float DEFAULT_MAXIMUM_NORMALIZED_NODE_SIZE = 0.8f;
		private static readonly Color EDGE_COLOR_MAX = Color.white;
		private static readonly Color EDGE_COLOR_MIN = new Color(1.0f, 1.0f, 1.0f, 0.1f);
		private static readonly int NODE_MAX_FONT_SIZE = 14;
		private static readonly int SELECTED_NODE_THICKNESS = 4;

		private static Color m_ActiveColour;
		private static Color m_NodeHeaderTextColour;
		private static Color m_SelectedNodeColour;
		private readonly GUIStyle m_NodeHeaderStyle;
		private readonly GUIStyle m_NodeRectStyle;
	}
}