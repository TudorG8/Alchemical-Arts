using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Tests.Performance
{
	public struct PhysicsEngineTestData : IComponentData
	{
		public Entity testObject;

		public Entity folder;

		public float2 position;

		public float2 bounds;
	}
}