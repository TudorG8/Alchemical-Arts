using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	[UpdateAfter(typeof(CreatePhysicsBodySystem))]
	partial struct CreatePhysicsCircleSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsCircleSetup>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

			foreach (var (circleSetup, entity) in SystemAPI.Query<PhysicsCircleSetup>().WithEntityAccess())
			{
				var parent = SystemAPI.GetComponent<PhysicsBodyState>(circleSetup.bodyEntity);
				var physicsShape = parent.physicsBody.CreateShape(circleSetup.circleGeometry, circleSetup.shapeDefinition);
				
				state.EntityManager.SetComponentData(entity,  new PhysicsShapeState() { physicsShape = physicsShape });
				state.EntityManager.SetComponentEnabled<PhysicsShapeState>(entity, true);
				
				commandBuffer.RemoveComponent<PhysicsCircleSetup>(entity);
			}
		}
	}
}