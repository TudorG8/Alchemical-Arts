using Unity.Entities;

namespace AlchemicalArts.Gameplay.Temperature.Groups
{
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
	public partial class TemperatureGroup : ComponentSystemGroup { }
}