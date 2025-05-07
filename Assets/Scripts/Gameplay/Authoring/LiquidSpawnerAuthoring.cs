using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Gameplay.Authoring
{
	public struct LiquidSpawner : IComponentData
	{
		public int Count;

		public int Limit;

		public Entity Liquid;

		public double Cooldown;

		public double Timer;
	}


	public class LiquidSpawnerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public WrigglerAuthoring WrigglerAuthoring { get; private set;}


		public class LiquidSpawnerAuthoringBaker : Baker<LiquidSpawnerAuthoring>
		{
			public override void Bake(LiquidSpawnerAuthoring authoring)
			{
				DependsOn(authoring.WrigglerAuthoring);

				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new LiquidSpawner()
				{
					Limit = authoring.WrigglerAuthoring.Limit,
					Liquid = GetEntity(authoring.WrigglerAuthoring.Liquid, TransformUsageFlags.Dynamic),
					Cooldown = authoring.WrigglerAuthoring.Cooldown,
				});
			}
		}
	}
}