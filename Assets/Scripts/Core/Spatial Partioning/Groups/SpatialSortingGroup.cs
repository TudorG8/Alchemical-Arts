using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Groups
{
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(SpatialPartioningGroup))]
	public partial class SpatialSortingGroup : ComponentSystemGroup { }
}
