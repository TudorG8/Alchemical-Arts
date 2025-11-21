using System.Diagnostics;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(CustomPhysicsInitializationGroup))]
	partial struct CreatePhysicsWorldSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldSetupComponent>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (physicsWorldSetup, entity) in SystemAPI.Query<PhysicsWorldSetupComponent>().WithEntityAccess())
			{
				var physicsWorld = PhysicsWorld.defaultWorld;// PhysicsWorld.Create(physicsWorldSetup.worldDefinition);
				var physicsWorldConfig = new PhysicsWorldConfigComponent() { physicsWorld = physicsWorld };
				commandBuffer.AddComponent(entity, physicsWorldConfig);
				commandBuffer.RemoveComponent<PhysicsWorldSetupComponent>(entity);
			}
			
			commandBuffer.Playback(state.EntityManager);
		}
	}
}