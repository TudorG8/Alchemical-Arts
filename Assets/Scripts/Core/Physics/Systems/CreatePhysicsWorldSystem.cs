using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	partial struct CreatePhysicsWorldSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldSetup>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (physicsWorldSetup, entity) in SystemAPI.Query<PhysicsWorldSetup>().WithEntityAccess())
			{
				var physicsWorld = PhysicsWorld.Create(physicsWorldSetup.worldDefinition);
				var physicsWorldConfig = new PhysicsWorldState() { physicsWorld = physicsWorld };
				commandBuffer.AddComponent(entity, physicsWorldConfig);
				commandBuffer.RemoveComponent<PhysicsWorldSetup>(entity);
			}
			
			commandBuffer.Playback(state.EntityManager);
		}
	}
}