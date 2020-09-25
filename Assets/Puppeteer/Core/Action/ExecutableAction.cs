using Puppeteer.Core.WorldState;

namespace Puppeteer.Core.Action
{
	public class ExecutableAction<TKey, TValue> : IAction<TKey, TValue>
	{
		public ExecutableAction(string _label, WorldStateModifier<TKey, TValue> _preconditions, WorldStateModifier<TKey, TValue> _effects, float _baseUtility)
		{
			m_Label = _label;
			m_Preconditions = _preconditions;
			m_Effects = _effects;
			m_BaseUtility = _baseUtility;
		}

		protected ExecutableAction()
		{
		}

		public virtual void Cancel()
		{
			m_CurrentActionState = ActionState.Inactive;
			m_OnFailedCallback?.Invoke();
		}

		public virtual void Enter(Agent<TKey, TValue> _executingAgent)
		{
			m_CurrentActionState = ActionState.Running;
		}

		public virtual void Execute()
		{
			m_CurrentActionState = ActionState.Completed;
		}

		public virtual void Exit()
		{
			m_CurrentActionState = ActionState.Inactive;
			m_OnCompletedCallback?.Invoke();
		}

		public ActionState GetActionState()
		{
			return m_CurrentActionState;
		}

		public WorldStateModifier<TKey, TValue> GetEffects()
		{
			return m_Effects;
		}

		public string GetLabel()
		{
			return m_Label;
		}

		public WorldStateModifier<TKey, TValue> GetPreconditions()
		{
			return m_Preconditions;
		}

		public float GetUtility()
		{
			return m_BaseUtility;
		}

		public void Initalise(System.Action _onCompletedCallback, System.Action _onFailedCallback)
		{
			m_OnCompletedCallback = _onCompletedCallback;
			m_OnFailedCallback = _onFailedCallback;
		}

		public virtual bool IsValid(Agent<TKey, TValue> _executingAgent)
		{
			return true;
		}

		protected float m_BaseUtility = 0;
		protected WorldStateModifier<TKey, TValue> m_Effects = null;
		protected string m_Label = default;
		protected System.Action m_OnCompletedCallback;
		protected System.Action m_OnFailedCallback;
		protected WorldStateModifier<TKey, TValue> m_Preconditions = null;
		protected ActionState m_CurrentActionState = ActionState.Inactive;
	}
}