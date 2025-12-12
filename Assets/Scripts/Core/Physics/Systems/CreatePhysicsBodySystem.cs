using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
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

			foreach (var (physicsBodySetup, localTransform, entity) in SystemAPI.Query<PhysicsBodySetupComponent, LocalTransform>().WithEntityAccess())
			{
				var definition = physicsBodySetup.bodyDefinition;
				definition.position = localTransform.Position.To2D();
				var physicsBody = physicsWorldConfig.physicsWorld.CreateBody(definition);

				SetEntity(physicsBody, entity);
				commandBuffer.AddComponent(entity, new PhysicsBodyConfigComponent() { physicsBody = physicsBody });
				commandBuffer.RemoveComponent<PhysicsBodySetupComponent>(entity);
			}

			commandBuffer.Playback(state.EntityManager);
		}

		private void SetEntity(PhysicsBody physicsBody, Entity entity) => physicsBody.userData = new PhysicsUserData { physicsMaskValue = entity.EncodeAsPhysicsMask() };

		private Entity GetEntity(PhysicsBody physicsBody) => physicsBody.userData.physicsMaskValue.DecodeFromPhysicsMask();
	}

	public static class EntityExtensions
	{
		public static PhysicsMask EncodeAsPhysicsMask(this Entity entity) => UnsafeUtility.As<Entity, PhysicsMask>(ref entity);
		public static Entity DecodeFromPhysicsMask(this PhysicsMask physicsMask) => UnsafeUtility.As<PhysicsMask, Entity>(ref physicsMask);
	}
}