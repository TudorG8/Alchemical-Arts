using Unity.Entities;
using UnityEngine;

class LiquidSpawnerAuthoring : MonoBehaviour
{
	public int limit;

	public GameObject liquid;

	public float cooldown;


	class LiquidSpawnerAuthoringBaker : Baker<LiquidSpawnerAuthoring>
	{
		public override void Bake(LiquidSpawnerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new LiquidSpawner()
			{
				limit = authoring.limit,
				liquid = GetEntity(authoring.liquid, TransformUsageFlags.Dynamic),
				cooldown = authoring.cooldown,
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