using NUnit.Framework;
using Puppeteer.Core;
using Puppeteer.Core.Action;
using Puppeteer.Core.Planning;
using Puppeteer.Core.WorldState;
using UnityEngine;

namespace Puppeteer.Tests
{
	public class PlanningTests
	{
		private enum GathererBoolKeys
		{
			HAS_TARGET_POSITION,
			AT_TARGET_POSITION,
			TARGET_IS_MUSHROOM,
			TARGET_IS_FISH,
			CARRIES_MUSHROOM,
			CARRIES_FISH,
			HAS_ENOUGH_MUSHROOMS,
			HAS_ENOUGH_FISH,
		}

		private enum GathererKeys
		{
			HAS_TARGET_POSITION,
			AT_TARGET_POSITION,
			TARGET_IS,
			CARRIES_MUSHROOM,
			CARRIES_FISH,
			HAS_ENOUGH_MUSHROOMS,
			HAS_ENOUGH_FISH,
		}

		[Test]
		public void GoToMushroomTest()
		{
			GameObject testGameObject = new GameObject();
			Agent<string, bool> agent = testGameObject.AddComponent<StringBoolAgent>();
			SortableGoal<string, bool> goal = new SortableGoal<string, bool> { { "AtTargetPosition", true }, { "TargetIsMushroom", true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<string, bool> { });

			agent.AddAction(new ExecutableAction<string, bool>(_label: "LookForMushroom",
																_preconditions: new WorldStateModifier<string, bool> { },
																_effects: new WorldStateModifier<string, bool> { { "HasTargetPosition", true }, { "AtTargetPosition", false }, { "TargetIsMushroom", true } },
																_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, bool>(_label: "MoveToPosition",
																_preconditions: new WorldStateModifier<string, bool> { { "HasTargetPosition", true }, { "AtTargetPosition", false } },
																_effects: new WorldStateModifier<string, bool> { { "HasTargetPosition", false }, { "AtTargetPosition", true } },
																_baseUtility: 5.0f));

			GoalPlanPair<string, bool> generatedGoalPlanPair = m_StringBoolPlanner.GenerateGoalPlanPairForAgent(agent);

			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 2);
			Assert.AreEqual("LookForMushroom", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
			Assert.AreEqual("MoveToPosition", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
		}

		[Test]
		public void MultipleLinearActionsToGoal()
		{
			GameObject testGameObject = new GameObject();
			Agent<string, bool> agent = testGameObject.AddComponent<StringBoolAgent>();
			SortableGoal<string, bool> goal = new SortableGoal<string, bool> { { "TargetIsDead", true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<string, bool> { { "TargetIsDead", false } });

			agent.AddAction(new ExecutableAction<string, bool>(_label: "Attack",
																_preconditions: new WorldStateModifier<string, bool> { { "WeaponIsLoaded", true } },
																_effects: new WorldStateModifier<string, bool> { { "TargetIsDead", true } },
																_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, bool>(_label: "Load Weapon",
																_preconditions: new WorldStateModifier<string, bool> { { "WeaponIsArmed", true } },
																_effects: new WorldStateModifier<string, bool> { { "WeaponIsLoaded", true } },
																_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, bool>(_label: "Draw Weapon",
																_preconditions: null,
																_effects: new WorldStateModifier<string, bool> { { "WeaponIsArmed", true } },
																_baseUtility: 5.0f));

			GoalPlanPair<string, bool> generatedGoalPlanPair = m_StringBoolPlanner.GenerateGoalPlanPairForAgent(agent);

			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 3);
			Assert.AreEqual("Draw Weapon", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
			Assert.AreEqual("Load Weapon", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
			Assert.AreEqual("Attack", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
		}

		[Test]
		public void MultipleLinearActionsToGoalWithBetterOption()
		{
			GameObject testGameObject = new GameObject();
			Agent<string, bool> agent = testGameObject.AddComponent<StringBoolAgent>();

			SortableGoal<string, bool> goal = new SortableGoal<string, bool> { { "TargetIsDead", true } };
			goal.Priority = 1;

			agent.AddGoal(goal);

			agent.SetWorkingMemory(new WorldState<string, bool> { { "TargetIsDead", false } });

			agent.AddAction(new ExecutableAction<string, bool>(_label: "Attack",
																_preconditions: new WorldStateModifier<string, bool> { { "WeaponIsLoaded", true } },
																_effects: new WorldStateModifier<string, bool> { { "TargetIsDead", true } },
																_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, bool>(_label: "Load Weapon",
																_preconditions: new WorldStateModifier<string, bool> { { "WeaponIsArmed", true } },
																_effects: new WorldStateModifier<string, bool> { { "WeaponIsLoaded", true } },
																_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, bool>(_label: "Quick Reload Weapon",
																_preconditions: new WorldStateModifier<string, bool> { { "WeaponIsArmed", true } },
																_effects: new WorldStateModifier<string, bool> { { "WeaponIsLoaded", true } },
																_baseUtility: 10.0f));
			agent.AddAction(new ExecutableAction<string, bool>(_label: "Draw Weapon",
																_preconditions: null,
																_effects: new WorldStateModifier<string, bool> { { "WeaponIsArmed", true } },
																_baseUtility: 5.0f));

			GoalPlanPair<string, bool> generatedGoalPlanPair = m_StringBoolPlanner.GenerateGoalPlanPairForAgent(agent);

			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 3);
			Assert.AreEqual("Draw Weapon", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
			Assert.AreEqual("Quick Reload Weapon", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
			Assert.AreEqual("Attack", generatedGoalPlanPair.PlanInstance.Pop().GetLabel());
		}

		[Test]
		public void MushroomGathererEnumBoolPerformance()
		{
			GameObject testGameObject = new GameObject();
			Agent<GathererBoolKeys, bool> agent = testGameObject.AddComponent<EnumBoolAgent>();
			SortableGoal<GathererBoolKeys, bool> goal = new SortableGoal<GathererBoolKeys, bool> { { GathererBoolKeys.HAS_ENOUGH_MUSHROOMS, true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<GathererBoolKeys, bool> { });

			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "LookForMushroom",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.HAS_TARGET_POSITION, true }, { GathererBoolKeys.AT_TARGET_POSITION, false }, { GathererBoolKeys.TARGET_IS_MUSHROOM, true } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "PickupMushroom",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.AT_TARGET_POSITION, true }, { GathererBoolKeys.TARGET_IS_MUSHROOM, true } },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.CARRIES_MUSHROOM, true }, { GathererBoolKeys.HAS_TARGET_POSITION, true }, { GathererBoolKeys.AT_TARGET_POSITION, false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "MoveToPosition",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.HAS_TARGET_POSITION, true }, { GathererBoolKeys.AT_TARGET_POSITION, false } },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.HAS_TARGET_POSITION, false }, { GathererBoolKeys.AT_TARGET_POSITION, true } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "DropOffMushroomAtHome",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.CARRIES_MUSHROOM, true }, { GathererBoolKeys.AT_TARGET_POSITION, true } },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.AT_TARGET_POSITION, false }, { GathererBoolKeys.HAS_ENOUGH_MUSHROOMS, true }, { GathererBoolKeys.CARRIES_MUSHROOM, false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "LookForFish",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.HAS_TARGET_POSITION, true }, { GathererBoolKeys.AT_TARGET_POSITION, false }, { GathererBoolKeys.TARGET_IS_FISH, true } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "PickupFish",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.AT_TARGET_POSITION, true }, { GathererBoolKeys.TARGET_IS_FISH, true } },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.CARRIES_FISH, true }, { GathererBoolKeys.HAS_TARGET_POSITION, true }, { GathererBoolKeys.AT_TARGET_POSITION, false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererBoolKeys, bool>(_label: "DropOffFishAtHome",
													_preconditions: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.CARRIES_FISH, true }, { GathererBoolKeys.AT_TARGET_POSITION, true } },
													_effects: new WorldStateModifier<GathererBoolKeys, bool> { { GathererBoolKeys.AT_TARGET_POSITION, false }, { GathererBoolKeys.HAS_ENOUGH_FISH, true }, { GathererBoolKeys.CARRIES_FISH, false } },
													_baseUtility: 5.0f));

			GoalPlanPair<GathererBoolKeys, bool> generatedGoalPlanPair = m_EnumBoolPlanner.GenerateGoalPlanPairForAgent(agent);
			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 5);
		}

		[Test]
		public void MushroomGathererEnumObjectPerformance()
		{
			GameObject testGameObject = new GameObject();
			Agent<GathererKeys, object> agent = testGameObject.AddComponent<EnumObjectAgent>();
			SortableGoal<GathererKeys, object> goal = new SortableGoal<GathererKeys, object> { { GathererKeys.HAS_ENOUGH_MUSHROOMS, true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<GathererKeys, object> { });

			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "LookForMushroom",
													_preconditions: new WorldStateModifier<GathererKeys, object> { },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.HAS_TARGET_POSITION, true }, { GathererKeys.AT_TARGET_POSITION, false }, { GathererKeys.TARGET_IS, "Mushroom" } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "PickupMushroom",
													_preconditions: new WorldStateModifier<GathererKeys, object> { { GathererKeys.AT_TARGET_POSITION, true }, { GathererKeys.TARGET_IS, "Mushroom" } },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.CARRIES_MUSHROOM, true }, { GathererKeys.HAS_TARGET_POSITION, true }, { GathererKeys.AT_TARGET_POSITION, false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "MoveToPosition",
													_preconditions: new WorldStateModifier<GathererKeys, object> { { GathererKeys.HAS_TARGET_POSITION, true }, { GathererKeys.AT_TARGET_POSITION, false } },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.HAS_TARGET_POSITION, false }, { GathererKeys.AT_TARGET_POSITION, true } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "DropOffMushroomAtHome",
													_preconditions: new WorldStateModifier<GathererKeys, object> { { GathererKeys.CARRIES_MUSHROOM, true }, { GathererKeys.AT_TARGET_POSITION, true } },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.AT_TARGET_POSITION, false }, { GathererKeys.HAS_ENOUGH_MUSHROOMS, true }, { GathererKeys.CARRIES_MUSHROOM, false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "LookForFish",
													_preconditions: new WorldStateModifier<GathererKeys, object> { },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.HAS_TARGET_POSITION, true }, { GathererKeys.AT_TARGET_POSITION, false }, { GathererKeys.TARGET_IS, "Fish" } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "PickupFish",
													_preconditions: new WorldStateModifier<GathererKeys, object> { { GathererKeys.AT_TARGET_POSITION, true }, { GathererKeys.TARGET_IS, "Fish" } },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.CARRIES_FISH, true }, { GathererKeys.HAS_TARGET_POSITION, true }, { GathererKeys.AT_TARGET_POSITION, false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<GathererKeys, object>(_label: "DropOffFishAtHome",
													_preconditions: new WorldStateModifier<GathererKeys, object> { { GathererKeys.CARRIES_FISH, true }, { GathererKeys.AT_TARGET_POSITION, true } },
													_effects: new WorldStateModifier<GathererKeys, object> { { GathererKeys.AT_TARGET_POSITION, false }, { GathererKeys.HAS_ENOUGH_FISH, true }, { GathererKeys.CARRIES_FISH, false } },
													_baseUtility: 5.0f));

			GoalPlanPair<GathererKeys, object> generatedGoalPlanPair = m_EnumObjectPlanner.GenerateGoalPlanPairForAgent(agent);
			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 5);
		}

		[Test]
		public void MushroomGathererStringObjectPerformance()
		{
			GameObject testGameObject = new GameObject();
			Agent<string, object> agent = testGameObject.AddComponent<StringObjectAgent>();
			SortableGoal<string, object> goal = new SortableGoal<string, object> { { "HasEnoughMushrooms", true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<string, object> { });

			agent.AddAction(new ExecutableAction<string, object>(_label: "LookForMushroom",
													_preconditions: new WorldStateModifier<string, object> { },
													_effects: new WorldStateModifier<string, object> { { "HasTargetPosition", true }, { "AtTargetPosition", false }, { "TargetIs", "Mushroom" } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, object>(_label: "PickupMushroom",
													_preconditions: new WorldStateModifier<string, object> { { "AtTargetPosition", true }, { "TargetIs", "Mushroom" } },
													_effects: new WorldStateModifier<string, object> { { "CarriesMushroom", true }, { "HasTargetPosition", true }, { "AtTargetPosition", false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, object>(_label: "MoveToPosition",
													_preconditions: new WorldStateModifier<string, object> { { "HasTargetPosition", true }, { "AtTargetPosition", false } },
													_effects: new WorldStateModifier<string, object> { { "HasTargetPosition", false }, { "AtTargetPosition", true } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, object>(_label: "DropOffMushroomAtHome",
													_preconditions: new WorldStateModifier<string, object> { { "CarriesMushroom", true }, { "AtTargetPosition", true } },
													_effects: new WorldStateModifier<string, object> { { "AtTargetPosition", false }, { "HasEnoughMushrooms", true }, { "CarriesMushroom", false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, object>(_label: "LookForFish",
													_preconditions: new WorldStateModifier<string, object> { },
													_effects: new WorldStateModifier<string, object> { { "HasTargetPosition", true }, { "AtTargetPosition", false }, { "TargetIs", "Fish" } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, object>(_label: "PickupFish",
													_preconditions: new WorldStateModifier<string, object> { { "AtTargetPosition", true }, { "TargetIs", "Fish" } },
													_effects: new WorldStateModifier<string, object> { { "CarriesFish", true }, { "HasTargetPosition", true }, { "AtTargetPosition", false } },
													_baseUtility: 5.0f));
			agent.AddAction(new ExecutableAction<string, object>(_label: "DropOffFishAtHome",
													_preconditions: new WorldStateModifier<string, object> { { "CarriesFish", true }, { "AtTargetPosition", true } },
													_effects: new WorldStateModifier<string, object> { { "AtTargetPosition", false }, { "HasEnoughFish", true }, { "CarriesFish", false } },
													_baseUtility: 5.0f));

			GoalPlanPair<string, object> generatedGoalPlanPair = m_StringObjectPlanner.GenerateGoalPlanPairForAgent(agent);
			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 5);
		}

		[SetUp]
		public void Setup()
		{
			m_StringBoolPlanner = new Planner<string, bool>();
			m_StringObjectPlanner = new Planner<string, object>();
			m_EnumObjectPlanner = new Planner<GathererKeys, object>();
			m_EnumBoolPlanner = new Planner<GathererBoolKeys, bool>();
		}

		[Test]
		public void SingleActionToGoal()
		{
			GameObject testGameObject = new GameObject();
			Agent<string, bool> agent = testGameObject.AddComponent<StringBoolAgent>();

			SortableGoal<string, bool> goal = new SortableGoal<string, bool> { { "TargetIsDead", true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<string, bool> { { "TargetIsDead", false } });

			agent.AddAction(new ExecutableAction<string, bool>(_label: "Attack",
																_preconditions: null,
																_effects: new WorldStateModifier<string, bool> { { "TargetIsDead", true } },
																_baseUtility: 5.0f));

			GoalPlanPair<string, bool> generatedGoalPlanPair = m_StringBoolPlanner.GenerateGoalPlanPairForAgent(agent);

			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 1);
			Assert.AreEqual("Attack", generatedGoalPlanPair.PlanInstance.Peek().GetLabel());
		}

		[TearDown]
		public void Teardown()
		{
		}

		[Test]
		public void WorldStateAlreadyFulfilsGoal()
		{
			GameObject testGameObject = new GameObject();
			Agent<string, bool> agent = testGameObject.AddComponent<StringBoolAgent>();
			SortableGoal<string, bool> goal = new SortableGoal<string, bool> { { "TargetIsDead", true } };
			goal.Priority = 1;

			agent.AddGoal(goal);
			agent.SetWorkingMemory(new WorldState<string, bool> { { "TargetIsDead", true } });
			GoalPlanPair<string, bool> generatedGoalPlanPair = m_StringBoolPlanner.GenerateGoalPlanPairForAgent(agent);

			Assert.True(generatedGoalPlanPair.PlanInstance.Count == 0);
			Assert.True(agent.GetWorkingMemory().Equals(goal));
		}

		private Planner<GathererBoolKeys, bool> m_EnumBoolPlanner;
		private Planner<GathererKeys, object> m_EnumObjectPlanner;
		private Planner<string, bool> m_StringBoolPlanner;
		private Planner<string, object> m_StringObjectPlanner;

		private sealed class EnumBoolAgent : Agent<GathererBoolKeys, bool> { }

		private sealed class EnumObjectAgent : Agent<GathererKeys, object> { }

		private sealed class StringBoolAgent : Agent<string, bool> { }

		private sealed class StringObjectAgent : Agent<string, object> { }
	}
}