using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
public partial class TemperatureGroup : ComponentSystemGroup { }