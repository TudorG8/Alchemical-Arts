using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Naming.Authoring
{
	public class EntityNameAuthoring : MonoBehaviour 
	{
		public class EntityNameBaker : Baker<EntityNameAuthoring>
		{
			public override void Bake(EntityNameAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new _EntityNameData() { Value = authoring.gameObject.name});
			}
		}
	}
}