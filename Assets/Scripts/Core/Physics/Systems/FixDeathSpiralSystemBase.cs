using Unity.Burst;
using Unity.Entities;
using static Unity.Entities.RateUtils;

partial class FixDeathSpiralSystemBase : SystemBase
{
    protected override void OnCreate()
    {
        var fixedStepGroup = World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
        fixedStepGroup.RateManager = new FixedRateSimpleManager(1f / 60f);
    }

    protected override void OnUpdate()
    {
        Enabled = false;
    }
}