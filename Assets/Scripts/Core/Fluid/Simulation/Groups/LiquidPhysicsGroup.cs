using Unity.Entities;

namespace PotionCraft.Core.Fluid.Simulation.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	public partial class LiquidPhysicsGroup : ComponentSystemGroup { }
}
