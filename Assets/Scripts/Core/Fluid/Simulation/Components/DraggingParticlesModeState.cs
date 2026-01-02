using PotionCraft.Core.Fluid.Simulation.Models;
using Unity.Entities;

namespace PotionCraft.Core.Fluid.Simulation.Components
{
	public struct DraggingParticlesModeState : IComponentData
	{
		public DraggingParticlesMode mode;
	}
}