using Puppeteer.Core.WorldState;

namespace Puppeteer.Core.Action
{
	public enum ActionState
	{
		Inactive,
		Running,
		Completed,
		Failed
	}

	public interface IAction<TKey, TValue>
	{
		void Cancel();

		void Enter(Agent<TKey, TValue> _executingAgent);

		void Execute();

		void Exit();

		ActionState GetActionState();

		WorldStateModifier<TKey, TValue> GetEffects();

		string GetLabel();

		WorldStateModifier<TKey, TValue> GetPreconditions();

		float GetUtility();

		void Initalise(System.Action _onCompletedCallback, System.Action _onFailedCallback);

		bool IsValid(Agent<TKey, TValue> _executingAgent);
	}
}