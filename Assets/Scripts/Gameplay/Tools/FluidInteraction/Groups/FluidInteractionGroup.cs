using AlchemicalArts.Core.Fluid.Simulation.Groups;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(FluidPhysicsGroup))]
	[UpdateBefore(typeof(FluidWritebackGroup))]
	public partial class FluidInteractionGroup : ComponentSystemGroup { }
}
