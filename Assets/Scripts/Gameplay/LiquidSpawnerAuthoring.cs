using Unity.Entities;
using UnityEngine;

class LiquidSpawnerAuthoring : MonoBehaviour
{
	public WrigglerAuthoring wrigglerAuthoring;


	class LiquidSpawnerAuthoringBaker : Baker<LiquidSpawnerAuthoring>
	{
		public override void Bake(LiquidSpawnerAuthoring authoring)
		{
			DependsOn(authoring.wrigglerAuthoring);

			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new LiquidSpawner()
			{
				limit = authoring.wrigglerAuthoring.limit,
				liquid = GetEntity(authoring.wrigglerAuthoring.liquid, TransformUsageFlags.Dynamic),
				cooldown = authoring.wrigglerAuthoring.cooldown,
			});
		}
	}
}

public struct LiquidSpawner : IComponentData
{
	public int count;

	public int limit;

	public Entity liquid;

	public double cooldown;

	public double timer;
}