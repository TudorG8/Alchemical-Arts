using PotionCraft.Core.Physics.Groups;
using Unity.Entities;
using Unity.Physics.Systems;
using static Unity.Entities.RateUtils;

[UpdateInGroup(typeof(PhysicsInitializationGroup), OrderFirst = true)]
partial class FixDeathSpiralSystemBase : SystemBase
{
	protected override void OnCreate()
	{
		var fixedStepGroup = World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
		fixedStepGroup.RateManager = new FixedRateSimpleManager(1f / 120f);

		
	}

	protected override void OnUpdate()
	{
		var group = World.GetExistingSystemManaged<PhysicsSystemGroup>();
		group.Enabled = false;

		Enabled = false;
	}
}