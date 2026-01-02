using PotionCraft.Core.Fluid.Interaction.Models;
using Unity.Entities;

namespace PotionCraft.Core.Fluid.Interaction.Components
{
	public struct DraggingParticlesModeState : IComponentData
	{
		public DraggingParticlesMode mode;
	}
}