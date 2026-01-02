using Unity.Entities;

namespace PotionCraft.Core.SpatialPartioning.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
	[UpdateAfter(typeof(BeginFixedStepSimulationEntityCommandBufferSystem))]
	public partial class SpatialPartioningGroup : ComponentSystemGroup { }
}
