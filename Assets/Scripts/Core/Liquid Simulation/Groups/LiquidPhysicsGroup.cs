using Unity.Entities;

namespace PotionCraft.Core.LiquidSimulation.Groups
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	public partial class LiquidPhysicsGroup : ComponentSystemGroup { }
}
