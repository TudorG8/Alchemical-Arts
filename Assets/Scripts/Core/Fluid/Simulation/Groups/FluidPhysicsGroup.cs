using Unity.Entities;
using Unity.Physics.Systems;

namespace AlchemicalArts.Core.Fluid.Simulation.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(PhysicsSystemGroup))]
	public partial class FluidPhysicsGroup : ComponentSystemGroup { }
}
