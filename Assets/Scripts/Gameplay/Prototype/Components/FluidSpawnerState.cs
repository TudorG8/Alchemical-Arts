using Unity.Entities;

namespace AlchemicalArts.Gameplay.Prototype.Components
{
	public struct FluidSpawnerState : IComponentData
	{
		public int count;

		public double timer;
	}
}