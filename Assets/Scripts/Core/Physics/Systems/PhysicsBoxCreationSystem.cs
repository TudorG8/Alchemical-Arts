using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	[UpdateAfter(typeof(PhysicsBodyCreationSystem))]
	partial struct PhysicsBoxCreationSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (physicsShapeSetup, entity) in SystemAPI.Query<PhysicsBoxSetup>().WithEntityAccess())
			{
				var parent = SystemAPI.GetComponent<PhysicsBodyState>(physicsShapeSetup.bodyEntity);
				var buffer = SystemAPI.GetBuffer<PolygonGeometryData>(entity);

				foreach(var geometry in buffer)
				{
					parent.physicsBody.CreateShape(geometry.geometry, physicsShapeSetup.shapeDefinition);
				}
				commandBuffer.RemoveComponent<PhysicsBoxSetup>(entity);
			}

			commandBuffer.Playback(state.EntityManager);
		}
	}
}