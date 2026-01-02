using Unity.Entities;

namespace AlchemicalArts.Core.Input.Groups
{
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
	public partial class InputSystemGroup : ComponentSystemGroup { }
}