using AlchemicalArts.Core.SpatialPartioning.Groups;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(SpatialPartioningGroup))]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
public partial class TemperatureGroup : ComponentSystemGroup { }