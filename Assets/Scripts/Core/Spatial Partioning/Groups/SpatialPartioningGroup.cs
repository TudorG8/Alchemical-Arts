using Unity.Entities;
using Unity.Physics.Systems;

namespace AlchemicalArts.Core.SpatialPartioning.Groups
{
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
	public partial class SpatialPartioningGroup : ComponentSystemGroup { }
}
