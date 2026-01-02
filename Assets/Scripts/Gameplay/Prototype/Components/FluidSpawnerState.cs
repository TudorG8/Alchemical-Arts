using Unity.Entities;

namespace PotionCraft.Gameplay.Prototype.Components
{
	public struct FluidSpawnerState : IComponentData
	{
		public int count;

		public double timer;
	}
}