using Puppeteer.Core.Planning;
using Puppeteer.UI.External.GraphVisualizer;
using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI
{
	internal class VisualiserBaseNode : Node
	{
		protected VisualiserBaseNode(HierarchyNode<string, object> _hierarchyNode, NodeType _typeOfNode, string _hexColour = m_DefaultColourString, float _weight = 1, bool _isActive = false)
			: base(_content: _hierarchyNode, _typeOfNode, _weight, _isActive)
		{
			m_HierarchyNode = _hierarchyNode;

			ColorUtility.TryParseHtmlString(_hexColour, out var colour);
			m_Colour = colour;
		}

		public virtual void DrawInspector(PuppeteerPlanVisualiser _visualiser)
		{
			GUILayout.Label(m_HierarchyNode.GetExecutableAction()?.ToString(), EditorStyles.whiteLargeLabel);
			EditorGUILayout.Space();
		}

		public override Color GetColor()
		{
			return m_Colour;
		}

		protected readonly HierarchyNode<string, object> m_HierarchyNode;
		protected Color m_Colour;
		private const string m_DefaultColourString = "#576574";
	}
}