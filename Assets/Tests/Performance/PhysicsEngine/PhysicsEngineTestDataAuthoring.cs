using AlchemicalArts.Shared.Extensions;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Tests.Performance
{
	class PhysicsEngineTestDataAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public GameObject Folder { get; private set; }

		[field: SerializeField]
		public GameObject TestObject { get; private set; }

		[field: SerializeField]
		public BoxCollider2D BoxBounds { get; private set; }
	}

	class PhysicsEngineTestDataAuthoringBaker : Baker<PhysicsEngineTestDataAuthoring>
	{
		public override void Bake(PhysicsEngineTestDataAuthoring authoring)
		{
			DependsOn(authoring.TestObject);
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new PhysicsEngineTestData()
			{
				testObject = GetEntity(authoring.TestObject, TransformUsageFlags.Dynamic),
				folder = GetEntity(authoring.Folder, TransformUsageFlags.None),
				position = authoring.BoxBounds.bounds.center.Tofloat2(),
				bounds = authoring.BoxBounds.bounds.size.Tofloat2(),
			});
		}
	}
}