using PotionCraft.Core.Physics.Groups;
using Unity.Entities;
using Unity.Physics.Systems;
using static Unity.Entities.RateUtils;

[UpdateInGroup(typeof(PhysicsInitializationGroup), OrderFirst = true)]
partial struct PhysicsSimulationRateOverrideSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		var fixedStepGroup = state.World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
		fixedStepGroup.RateManager = new FixedRateSimpleManager(1f / 120f);
	}

	public void OnUpdate(ref SystemState state)
	{
		var group = state.World.GetExistingSystemManaged<PhysicsSystemGroup>();
		group.Enabled = false;

		state.Enabled = false;
	}
}