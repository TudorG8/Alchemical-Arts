using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EntityHierarchyAuthoring : MonoBehaviour 
{
	[SerializeField]
	private bool includeChildren = true;
	
	
	public class EntityHierarchyBaker : Baker<EntityHierarchyAuthoring>
	{
		public override void Bake(EntityHierarchyAuthoring authoring)
		{
			var entity = GetEntity(authoring.transform, TransformUsageFlags.Dynamic);
			var buffer = AddBuffer<TransformLinkData>(entity);

			if (authoring.includeChildren)
			{
				ApplyHirarchyRecursively(buffer, authoring.transform);
			}
			else
			{
				ApplyHierarchy(buffer, authoring.transform);
			}
		}

		private void ApplyHirarchyRecursively(DynamicBuffer<TransformLinkData> buffer, Transform transform)
		{
			ApplyHierarchy(buffer, transform);
			foreach(Transform child in transform)
			{
				if (child.GetComponent<EntityHierarchyAuthoring>() != null)
					continue;
				ApplyHirarchyRecursively(buffer, child);
			}
		}

		private void ApplyHierarchy(DynamicBuffer<TransformLinkData> buffer, Transform transform)
		{
			var entity = GetEntity(transform, TransformUsageFlags.Dynamic);

			var parentEntity = transform.parent == null
				? Entity.Null
				: GetEntity(transform.parent, TransformUsageFlags.Dynamic);

			buffer.Add(new TransformLinkData
			{
				Parent = parentEntity,
				Child = entity,
				Name = transform.gameObject.name,
				LocalPosition = transform.localPosition
			});
		}
	}
}

public struct TransformLinkData : IBufferElementData
{
	public Entity Parent;

	public Entity Child;

	public FixedString64Bytes Name;

	public float3 LocalPosition;
}

public struct _EntityName : IComponentData, IEnableableComponent
{
	public FixedString64Bytes name;
}