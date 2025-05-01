using Unity.Entities;
using UnityEngine;

class LiquidAuthoring : MonoBehaviour
{
	class LiquidAuthoringBaker : Baker<LiquidAuthoring>
	{
		public override void Bake(LiquidAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Liquid());
		}
	}
}

public struct Liquid : IComponentData
{
	
}