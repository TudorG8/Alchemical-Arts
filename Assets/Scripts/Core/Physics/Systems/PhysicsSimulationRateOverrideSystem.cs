using AlchemicalArts.Core.Physics.Groups;
using Unity.Entities;
using static Unity.Entities.RateUtils;

[UpdateInGroup(typeof(PhysicsInitializationGroup), OrderFirst = true)]
partial struct PhysicsSimulationRateOverrideSystem : ISystem
{
	public void OnCreate(ref SystemState state)
	{
		var fixedStepGroup = state.World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
		fixedStepGroup.RateManager = new FixedRateSimpleManager(1f / 120f);
	}
}