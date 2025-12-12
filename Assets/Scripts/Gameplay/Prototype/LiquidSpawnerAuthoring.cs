using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Gameplay.Authoring
{
	public struct _LiquidSpawner : IComponentData
	{
		public int Count;

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
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new _LiquidSpawner());
			}
		}
	}
}