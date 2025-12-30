using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Gameplay.Authoring
{
	public struct FluidSpawner : IComponentData
	{
		public int max;

		public int count;

		public double timer;

		public float delay;

		public Entity fluid;
	}


	public class FluidSpawnerAuthoring : MonoBehaviour
	{
		public int max;

		public float delay;

		public GameObject fluid;


		public class FluidSpawnerAuthoringBaker : Baker<FluidSpawnerAuthoring>
		{
			public override void Bake(FluidSpawnerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new FluidSpawner()
				{
					max = authoring.max,
					delay = authoring.delay,
					fluid = GetEntity(authoring.fluid, TransformUsageFlags.Dynamic)
				});
			}
		}
	}
}