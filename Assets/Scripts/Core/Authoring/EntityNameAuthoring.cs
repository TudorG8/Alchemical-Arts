using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;


public class EntityNameAuthoring : MonoBehaviour 
{
	public class EntityNameBaker : Baker<EntityNameAuthoring>
	{
		public override void Bake(EntityNameAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new _EntityName() { name = authoring.gameObject.name});
		}
	}
}