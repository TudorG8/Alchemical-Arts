using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	[UpdateAfter(typeof(CreatePhysicsBodySystem))]
	partial struct CreatePhysicsBoxSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (physicsShapeSetup, entity) in SystemAPI.Query<PhysicsBoxSetupComponent>().WithEntityAccess())
			{
				var parent = SystemAPI.GetComponent<PhysicsBodyConfigComponent>(physicsShapeSetup.bodyEntity);
				var buffer = SystemAPI.GetBuffer<PolygonGeometryBufferData>(entity);

				foreach(var geometry in buffer)
				{
					var physicsShape = parent.physicsBody.CreateShape(geometry.geometry, physicsShapeSetup.shapeDefinition);
				}
				// commandBuffer.AddComponent(entity, new PhysicsBoxConfigComponent() { physicsShape = physicsShape });
				commandBuffer.RemoveComponent<PhysicsBoxSetupComponent>(entity);
			}

			commandBuffer.Playback(state.EntityManager);
		}
	}
}