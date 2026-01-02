using Unity.Entities;

namespace AlchemicalArts.Gameplay.Prototype.Components
{
	public struct FluidSpawnerConfig : IComponentData
	{
		public int max;

		public float delay;

		public Entity fluid;
	}
}