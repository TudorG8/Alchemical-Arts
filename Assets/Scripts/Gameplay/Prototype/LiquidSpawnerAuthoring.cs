using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Gameplay.Authoring
{
	public struct LiquidSpawner : IComponentData
	{
		public int max;

		public int count;

		public double timer;

		public float delay;

		public Entity liquid;
	}


	public class LiquidSpawnerAuthoring : MonoBehaviour
	{
		public int max;

		public float delay;

		public GameObject liquid;


		public class LiquidSpawnerAuthoringBaker : Baker<LiquidSpawnerAuthoring>
		{
			public override void Bake(LiquidSpawnerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new LiquidSpawner()
				{
					max = authoring.max,
					delay = authoring.delay,
					liquid = GetEntity(authoring.liquid, TransformUsageFlags.Dynamic)
				});
			}
		}
	}
}