using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class PlanItem : VisualElement
	{
		public enum PlanItemType
		{
			ActiveGoal,
			ActiveAction,
			InactiveAction,
		}

		public PlanItem(PlanItemType _planItemType, string _labelLext)
		{
			m_PlanItemType = _planItemType;
			m_LabelText = _labelLext;

			Init();
		}

		protected void Init()
		{
			AddToClassList("horizontalGroup");
			AddToClassList(USS_CLASS_NAME);

			Image icon = new Image();
			m_Label = new Label(m_LabelText);

			switch (m_PlanItemType)
			{
				case PlanItemType.ActiveGoal:
					var goalSprite = PuppeteerEditorResourceLoader.GoalIcon16;
					icon.image = goalSprite.texture;
					m_Label.AddToClassList("activeGoal");
					tooltip = "The current goal.";
					break;

				case PlanItemType.ActiveAction:
					var activeActionSprite = PuppeteerEditorResourceLoader.PlanActiveActionIcon64;
					icon.image = activeActionSprite.texture;
					m_Label.AddToClassList("activeAction");
					tooltip = "The action that is currently executed.";
					break;

				case PlanItemType.InactiveAction:
					tooltip = "An action that will be executed in the future.";
					break;

				default:
					break;
			}

			hierarchy.Add(icon);
			hierarchy.Add(m_Label);
		}

		public static readonly string USS_CLASS_NAME = "planItem";

		private readonly PlanItemType m_PlanItemType;
		private readonly string m_LabelText;

		private Label m_Label;
	}
}