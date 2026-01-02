using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Interaction.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(SpatialPartioningGroup))]
	[UpdateBefore(typeof(FluidPhysicsGroup))]
	public partial class FluidInteractionGroup : ComponentSystemGroup { }
}
