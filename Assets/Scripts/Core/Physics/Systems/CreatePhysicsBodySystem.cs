using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(CustomPhysicsInitializationGroup))]
	[UpdateAfter(typeof(CreatePhysicsWorldSystem))]
	partial struct CreatePhysicsBodySystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsBodySetupComponent>();
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var physicsWorldConfig = SystemAPI.GetSingleton<PhysicsWorldConfigComponent>();
			
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (physicsBodySetup, entity) in SystemAPI.Query<PhysicsBodySetupComponent>().WithEntityAccess())
			{
				var physicsBody = physicsWorldConfig.physicsWorld.CreateBody(physicsBodySetup.bodyDefinition);
				commandBuffer.AddComponent(entity, new PhysicsBodyConfigComponent() { physicsBody = physicsBody });
				commandBuffer.RemoveComponent<PhysicsBodySetupComponent>(entity);
			}

			commandBuffer.Playback(state.EntityManager);
		}
	}
}