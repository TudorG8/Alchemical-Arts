using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Simulation.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	public partial class FluidPhysicsGroup : ComponentSystemGroup { }
}
