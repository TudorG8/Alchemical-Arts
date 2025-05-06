using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EntityHierarchyAuthoring : MonoBehaviour {}

public class EntityHierarchyBaker : Baker<EntityHierarchyAuthoring>
{
	public override void Bake(EntityHierarchyAuthoring authoring)
	{
		ApplyHierarchy(authoring.transform);
	}

	private void ApplyHierarchy(Transform transform)
	{
		var entity = GetEntity(transform, TransformUsageFlags.Dynamic);
		if (transform.parent != null)
		{
			var parentEntity = GetEntity(transform.parent, TransformUsageFlags.Dynamic);
			AddComponent(entity, new NeedsReparenting 
			{
				target = parentEntity,
				localPosition = transform.localPosition
			});
		}

		AddComponent(entity, new _EntityName() { name = transform.gameObject.name});
	}
}

public struct NeedsReparenting : IComponentData
{
	public Entity target;

	public float3 localPosition;
}