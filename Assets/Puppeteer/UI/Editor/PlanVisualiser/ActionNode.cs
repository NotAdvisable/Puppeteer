using Puppeteer.Core;
using Puppeteer.Core.Configuration;
using Puppeteer.Core.Planning;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI
{
	internal class ActionNode : VisualiserBaseNode
	{
		public ActionNode(HierarchyNode<string, object> _hierarchyNode, float _weight = 0.5f, bool _isActive = false)
			: base(_hierarchyNode: _hierarchyNode,
				  _typeOfNode: _hierarchyNode.IsFinalNode() ? NodeType.FINAL : (_hierarchyNode.IsPartOfFoundPath() ? NodeType.FINALPATH : (_hierarchyNode.IsClosed() ? NodeType.CLOSED : NodeType.OPEN)),
				  _hexColour: string.Empty,
				  _weight: _weight,
				  _isActive: _isActive)
		{
			if (!m_ColoursInitialised)
			{
				_ = ColorUtility.TryParseHtmlString(m_PartOfPathColourString, out Color colour);
				m_PartOfPathColour = colour;

				_ = ColorUtility.TryParseHtmlString(m_ClosedColourString, out colour);
				m_ClosedColour = colour;

				_ = ColorUtility.TryParseHtmlString(m_OpenColourString, out colour);
				m_OpenColour = colour;

				_ = ColorUtility.TryParseHtmlString(m_FinalNodeColourString, out colour);
				m_FinalNodeColour = colour;

				m_ColoursInitialised = true;
			}

			System.Guid descriptionGUID = (m_HierarchyNode.GetExecutableAction() as PuppeteerExecutableAction).DescriptionGUID;
			m_ActionDescription = PuppeteerManager.Instance.GetActionDescription(descriptionGUID);
		}

		public override void DrawInspector(PuppeteerPlanVisualiser _visualiser)
		{
			PuppeteerEditorGUIHelper.DrawGraphInspectorHeader(PuppeteerEditorResourceLoader.ActionIcon32.texture, "Action", GetContentTypeName());
			DrawNodeInfo();

			_visualiser.ShowActionUtilityFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowActionUtilityFoldout, "Utility");
			if (_visualiser.ShowActionUtilityFoldout)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.LabelField(m_ActionDescription.BaseUtility.ToString());
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			_visualiser.ShowActionPreconditionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowActionPreconditionFoldout, "Preconditions");
			if (_visualiser.ShowActionPreconditionFoldout)
			{
				++EditorGUI.indentLevel;
				if (m_ActionDescription.Preconditions.Length == 0)
				{
					EditorGUILayout.LabelField("none");
				}
				else
				{
					foreach (var precondition in m_ActionDescription.Preconditions)
					{
						PuppeteerEditorGUIHelper.DrawHorizontal(precondition.Key, precondition.Value.ToString());
					}
				}
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			_visualiser.ShowActionEffectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowActionEffectsFoldout, "Effects");
			if (_visualiser.ShowActionEffectsFoldout)
			{
				++EditorGUI.indentLevel;
				foreach (var effect in m_ActionDescription.Effects)
				{
					PuppeteerEditorGUIHelper.DrawHorizontal(effect.Key, effect.Value.ToString());
				}
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			_visualiser.ShowActionGoalWorldStateAtNodeFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowActionGoalWorldStateAtNodeFoldout, "Goal World State at Node");
			if (_visualiser.ShowActionGoalWorldStateAtNodeFoldout)
			{
				++EditorGUI.indentLevel;
				foreach (KeyValuePair<string, object> goalWorldStateAtNodePair in m_HierarchyNode.GetSortedGoalWorldStateAtNode())
				{
					PuppeteerEditorGUIHelper.DrawHorizontal(goalWorldStateAtNodePair.Key, goalWorldStateAtNodePair.Value.ToString());
				}
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			_visualiser.ShowActionWorldStateAtNodeFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowActionWorldStateAtNodeFoldout, "Total Remaining Preconditions at Node");
			if (_visualiser.ShowActionWorldStateAtNodeFoldout)
			{
				++EditorGUI.indentLevel;
				if (m_HierarchyNode.GetSortedRemainingPreconditions().Count == 0)
				{
					EditorGUILayout.LabelField("none");
				}
				else
				{
					foreach (KeyValuePair<string, object> remainingPreconditions in m_HierarchyNode.GetSortedRemainingPreconditions())
					{
						PuppeteerEditorGUIHelper.DrawHorizontal(remainingPreconditions.Key, remainingPreconditions.Value.ToString());
					}
				}
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			EditorGUILayout.Space();
			PuppeteerEditorGUIHelper.DrawDivider();
			EditorGUILayout.Space();

			if (GUILayout.Button("Open Configuration"))
			{
				var window = PuppeteerConfiguratorWindow.CreateWindow();
				window.OpenEntryInTabOfType<ActionView>(m_ActionDescription.GUID);
			}
		}

		public override Color GetColor()
		{
#pragma warning disable IDE0066 //Use 'switch' expression (switch expressions aren't yet supported by Unity.
			switch (TypeOfNode)
			{
				case NodeType.FINALPATH:
					return m_PartOfPathColour;

				case NodeType.FINAL:
					return m_FinalNodeColour;

				case NodeType.OPEN:
					return m_OpenColour;

				case NodeType.CLOSED:
				default:
					return m_ClosedColour;
			}
#pragma warning restore IDE0066
		}

		public override string GetContentTypeName()
		{
			return m_HierarchyNode.GetExecutableAction()?.GetLabel();
		}

		private void DrawNodeInfo()
		{
			EditorGUILayout.Space();
			PuppeteerEditorGUIHelper.DrawDivider();
			EditorGUILayout.Space();

			switch (TypeOfNode)
			{
				case NodeType.FINALPATH:
					EditorGUILayout.LabelField(IsActive ? "Node is currently being executed!" : "Part of found plan.");
					break;

				case NodeType.OPEN:
					EditorGUILayout.LabelField("Node wasn't addressed. Others were better.");
					break;

				case NodeType.CLOSED:
					EditorGUILayout.LabelField("Node was discarded. Others were better.");
					break;

				case NodeType.FINAL:
					EditorGUILayout.LabelField(IsActive ? "Node is currently being executed!" : "First node of found plan.");
					break;

				default:
					break;
			}

			EditorGUILayout.Space();
			PuppeteerEditorGUIHelper.DrawDivider();
			EditorGUILayout.Space();
		}

		private const string m_ClosedColourString = "#576574";
		private const string m_FinalNodeColourString = "#00d2d3";
		private const string m_OpenColourString = "#8395a7";
		private const string m_PartOfPathColourString = "#10ac84";
		private static Color m_ClosedColour;
		private static bool m_ColoursInitialised = false;
		private static Color m_FinalNodeColour;
		private static Color m_OpenColour;
		private static Color m_PartOfPathColour;
		private readonly ActionDescription m_ActionDescription;
	}
}