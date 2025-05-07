using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Authoring
{
	public struct _TransformLinkData : IBufferElementData
	{
		public Entity Parent;

		public Entity Child;

		public FixedString64Bytes Name;
	}

	public struct _EntityNameData : IComponentData, IEnableableComponent
	{
		public FixedString64Bytes Value;
	}


	public class EntityHierarchyAuthoring : MonoBehaviour 
	{
		[SerializeField]
		private bool IncludeChildren { get; set; } = true;
		
		
		public class EntityHierarchyBaker : Baker<EntityHierarchyAuthoring>
		{
			public override void Bake(EntityHierarchyAuthoring authoring)
			{
				var entity = GetEntity(authoring.transform, TransformUsageFlags.Dynamic);
				var buffer = AddBuffer<_TransformLinkData>(entity);

				if (authoring.IncludeChildren)
				{
					ApplyHirarchyRecursively(buffer, authoring.transform);
				}
				else
				{
					ApplyHierarchy(buffer, authoring.transform);
				}
			}

			private void ApplyHirarchyRecursively(DynamicBuffer<_TransformLinkData> buffer, Transform transform)
			{
				ApplyHierarchy(buffer, transform);
				foreach(Transform child in transform)
				{
					if (child.GetComponent<EntityHierarchyAuthoring>() != null)
						continue;
					ApplyHirarchyRecursively(buffer, child);
				}
			}

			private void ApplyHierarchy(DynamicBuffer<_TransformLinkData> buffer, Transform transform)
			{
				var entity = GetEntity(transform, TransformUsageFlags.Dynamic);

				var parentEntity = transform.parent == null
					? Entity.Null
					: GetEntity(transform.parent, TransformUsageFlags.Dynamic);

				buffer.Add(new _TransformLinkData
				{
					Parent = parentEntity,
					Child = entity,
					Name = transform.gameObject.name
				});
			}
		}
	}
}