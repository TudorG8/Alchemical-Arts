using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Entities;
using static Unity.Entities.RateUtils;

[UpdateInGroup(typeof(CustomPhysicsInitializationGroup), OrderFirst = true)]
partial class FixDeathSpiralSystemBase : SystemBase
{
    protected override void OnCreate()
    {
        var fixedStepGroup = World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
        fixedStepGroup.RateManager = new FixedRateSimpleManager(1f / 120f);
    }

    protected override void OnUpdate()
    {
        Enabled = false;
    }
}