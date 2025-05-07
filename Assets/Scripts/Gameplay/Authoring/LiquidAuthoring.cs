using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Gameplay.Authoring
{
	public struct _LiquidTag : IComponentData { }

	public class LiquidAuthoring : MonoBehaviour
	{
		public class LiquidAuthoringBaker : Baker<LiquidAuthoring>
		{
			public override void Bake(LiquidAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new _LiquidTag());
			}
		}
	}
}