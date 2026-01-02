using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.Physics.Groups;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	public partial struct PhysicsWorldCreationSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldSetup>();
			state.RequireForUpdate<PhysicsWorldState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

			foreach (var (physicsWorldSetup, entity) in SystemAPI.Query<PhysicsWorldSetup>().WithEntityAccess())
			{
				var physicsWorld = PhysicsWorld.Create(physicsWorldSetup.worldDefinition);
				
				state.EntityManager.SetComponentData(entity,  new PhysicsWorldState() { physicsWorld = physicsWorld });
				
				commandBuffer.RemoveComponent<PhysicsWorldSetup>(entity);
			}
		}
	}
}