using AlchemicalArts.Core.Naming.BakingComponents;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Naming.Authoring
{
	public class EntityHierarchyAuthoring : MonoBehaviour 
	{
		[field:SerializeField]
		public bool IncludeChildren { get; private set; } = true;

		[field:SerializeField]
		public bool IncludeName { get; private set; } = true;
	}

	public class EntityHierarchyBaker : Baker<EntityHierarchyAuthoring>
	{
		public override void Bake(EntityHierarchyAuthoring authoring)
		{
			var entity = GetEntity(authoring.transform, TransformUsageFlags.Dynamic);
			var buffer = AddBuffer<TransformLinkBakingData>(entity);

			if (authoring.IncludeChildren)
			{
				ApplyHirarchyRecursively(buffer, authoring.transform, authoring.IncludeName);
			}
			else
			{
				ApplyHierarchy(buffer, authoring.transform, authoring.IncludeName);
			}
		}

		private void ApplyHirarchyRecursively(DynamicBuffer<TransformLinkBakingData> buffer, Transform transform, bool applyName)
		{
			ApplyHierarchy(buffer, transform, applyName);
			foreach(Transform child in transform)
			{
				if (child.GetComponent<EntityHierarchyAuthoring>() != null)
					continue;
				ApplyHirarchyRecursively(buffer, child, applyName);
			}
		}

		private void ApplyHierarchy(DynamicBuffer<TransformLinkBakingData> buffer, Transform transform, bool applyName)
		{
			var entity = GetEntity(transform, TransformUsageFlags.Dynamic);

			var parentEntity = transform.parent == null
				? Entity.Null
				: GetEntity(transform.parent, TransformUsageFlags.Dynamic);

			buffer.Add(new TransformLinkBakingData
			{
				Parent = parentEntity,
				Child = entity,
				applyName = applyName,
				Name = transform.gameObject.name
			});
		}
	}
}