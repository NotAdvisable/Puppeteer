using Puppeteer.Core.Configuration;
using Puppeteer.Core.Planning;
using UnityEditor;
using UnityEngine;

namespace Puppeteer.UI
{
	internal class GoalNode : VisualiserBaseNode
	{
		public GoalNode(GoalDescription _goalDescription, HierarchyNode<string, object> _hierarchyNode, float _weight = 1, bool _isActive = false)
			: base(_hierarchyNode: _hierarchyNode, _typeOfNode: NodeType.GOAL, _hexColour: m_GoalColourString, _weight: _weight, _isActive: _isActive)
		{
			m_GoalDescription = _goalDescription;
		}

		public override void DrawInspector(PuppeteerPlanVisualiser _visualiser)
		{
			PuppeteerEditorGUIHelper.DrawGraphInspectorHeader(PuppeteerEditorResourceLoader.GoalIcon32.texture, "Goal", GetContentTypeName());

			EditorGUILayout.Space();
			PuppeteerEditorGUIHelper.DrawDivider();
			EditorGUILayout.Space();

			_visualiser.ShowGoalUtilityFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowGoalUtilityFoldout, "Utility");
			if (_visualiser.ShowGoalUtilityFoldout)
			{
				++EditorGUI.indentLevel;
				PuppeteerEditorGUIHelper.DrawHorizontal("Base Utility", m_GoalDescription.BaseUtility.ToString());

				++EditorGUI.indentLevel;
				foreach (var utilityPart in m_GoalDescription.UtilityParts)
				{
					EditorGUILayout.LabelField(string.Format("{0} {1} * {2}", PuppeteerEditorHelper.UtilityOperatorToString(utilityPart.UtilityOperator),
						utilityPart.WorldStateName.ToString(), utilityPart.CurveMultiplier));
				}
				--EditorGUI.indentLevel;

				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			_visualiser.ShowGoalPartsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_visualiser.ShowGoalPartsFoldout, "Goal Parts");
			if (_visualiser.ShowGoalPartsFoldout)
			{
				++EditorGUI.indentLevel;
				foreach (var goalPart in m_GoalDescription.GoalParts)
				{
					PuppeteerEditorGUIHelper.DrawHorizontal(goalPart.Key, goalPart.Value.ToString());
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
				window.OpenEntryInTabOfType<GoalView>(m_GoalDescription.GUID);
			}
		}

		public override string GetContentTypeName()
		{
			return m_GoalDescription.DisplayName;
		}

		private const string m_GoalColourString = "#FF9F43";
		private readonly GoalDescription m_GoalDescription;
	}
}