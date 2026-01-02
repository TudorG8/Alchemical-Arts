using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.SpatialPartioning.Groups;
using Unity.Entities;

namespace PotionCraft.Core.Fluid.Interaction.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(SpatialPartioningGroup))]
	[UpdateBefore(typeof(FluidPhysicsGroup))]
	public partial class FluidInteractionGroup : ComponentSystemGroup { }
}
