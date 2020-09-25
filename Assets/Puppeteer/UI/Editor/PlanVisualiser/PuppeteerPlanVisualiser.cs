using Puppeteer.Core;
using Puppeteer.Core.Configuration;
using Puppeteer.Core.Planning;
using Puppeteer.UI.External.GraphVisualizer;
using System.Collections.Generic;

namespace Puppeteer.UI
{
	public class PuppeteerPlanVisualiser : Graph
	{
		public PuppeteerPlanVisualiser()
		{
		}

		public bool ShowActionEffectsFoldout { get; set; } = true;
		public bool ShowActionGoalWorldStateAtNodeFoldout { get; set; } = true;
		public bool ShowActionPreconditionFoldout { get; set; } = true;
		public bool ShowActionUtilityFoldout { get; set; } = true;
		public bool ShowActionWorldStateAtNodeFoldout { get; set; } = true;
		public bool ShowGoalPartsFoldout { get; set; } = true;
		public bool ShowGoalUtilityFoldout { get; set; } = true;

		public void OverrideTargetAgent(PuppeteerAgent _agent)
		{
			if (m_Agent != null)
			{
				m_Agent.OnPlanProgressed -= UpdatePlanForAgent;
			}

			m_Agent = _agent;
			if (m_Agent != null)
			{
				m_Agent.OnPlanProgressed += UpdatePlanForAgent;
			}
		}

		public void UpdatePlanForAgent()
		{
			HierarchyNode<string, object> rootHierarchyNode = m_Agent.GetRootHierarchyNode();
			PuppeteerGoal activeGoal = m_Agent.GetActiveGoal();

			if (rootHierarchyNode == null || activeGoal == null)
			{
				// plan hasn't been generated yet or the agent doesn't have a plan.
				Clear();
				m_RootNode = null;
				return;
			}

			GoalDescription activeGoalDescription = PuppeteerManager.Instance.GetGoalDescription(activeGoal.DescriptionGUID);

			m_RootNode = new GoalNode(activeGoalDescription, rootHierarchyNode);

			CreateActionNodesRecursive(ref rootHierarchyNode, ref m_RootNode);

			int currentIndexInPlan = m_Agent.GetIndexInPlan();

			Node rootNode = m_RootNode;
			Node activeNode = FindFinalNodeRecursive(ref rootNode);

			for (int i = 0; i < currentIndexInPlan; ++i)
			{
				activeNode = activeNode.Parent;
			}

			activeNode.IsActive = true;

			while (activeNode != null)
			{
				activeNode.Weight = float.MaxValue;
				activeNode = activeNode.Parent;
			}

			OnVisualiserContentChanged?.Invoke();
		}

		protected override IEnumerable<Node> GetChildren(Node _node)
		{
			if (_node is VisualiserBaseNode baseNode)
			{
				return baseNode.Children;
			}

			return new List<Node>();
		}

		protected override void Populate()
		{
			UpdatePlanForAgent();

			if (m_RootNode != null)
			{
				AddNodeHierarchy(m_RootNode);
			}
		}

		private void CreateActionNodesRecursive(ref HierarchyNode<string, object> _parentNode, ref VisualiserBaseNode _parentVisualiserNode)
		{
			for (int i = 0; i < _parentNode.GetChildCount(); ++i)
			{
				var hierarchyNode = _parentNode.GetChildren()[i];

				VisualiserBaseNode visualiserNode = new ActionNode(hierarchyNode, _weight: hierarchyNode.IsPartOfFoundPath() ? 1.0f : 0.0f);
				_parentVisualiserNode.AddChild(visualiserNode);

				CreateActionNodesRecursive(ref hierarchyNode, ref visualiserNode);
			}
		}

		private Node FindFinalNodeRecursive(ref Node _parentNode)
		{
			for (int i = 0; i < _parentNode.Children.Count; ++i)
			{
				Node childNode = _parentNode.Children[i];
				if (childNode.TypeOfNode == Node.NodeType.FINAL)
				{
					return childNode;
				}
				else if (childNode.TypeOfNode == Node.NodeType.FINALPATH)
				{
					return FindFinalNodeRecursive(ref childNode);
				}
			}

			return null;
		}

		public System.Action OnVisualiserContentChanged;

		private PuppeteerAgent m_Agent = null;
		private VisualiserBaseNode m_RootNode = null;
	}
}