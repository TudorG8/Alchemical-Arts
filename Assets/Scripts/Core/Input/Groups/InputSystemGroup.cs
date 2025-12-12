using Unity.Entities;

namespace PotionCraft.Core.Input.Groups
{
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
	public partial class InputSystemGroup : ComponentSystemGroup { }
}