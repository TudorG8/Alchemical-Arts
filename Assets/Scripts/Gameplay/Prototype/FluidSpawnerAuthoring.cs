using PotionCraft.Gameplay.Prototype.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Gameplay.Prototype.Authoring
{
	public class FluidSpawnerAuthoring : MonoBehaviour
	{
		public int max;

		public float delay;

		public GameObject fluid;
	}

	public class FluidSpawnerAuthoringBaker : Baker<FluidSpawnerAuthoring>
	{
		public override void Bake(FluidSpawnerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FluidSpawnerState());
			AddComponent(entity, new FluidSpawnerConfig()
			{
				max = authoring.max,
				delay = authoring.delay,
				fluid = GetEntity(authoring.fluid, TransformUsageFlags.Dynamic)
			});
		}
	}
}