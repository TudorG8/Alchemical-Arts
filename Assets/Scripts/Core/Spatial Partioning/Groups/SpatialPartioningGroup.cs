using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
	[UpdateAfter(typeof(BeginFixedStepSimulationEntityCommandBufferSystem))]
	public partial class SpatialPartioningGroup : ComponentSystemGroup { }
}
