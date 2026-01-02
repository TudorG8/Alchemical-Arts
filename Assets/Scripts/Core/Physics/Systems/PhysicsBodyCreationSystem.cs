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
	[UpdateAfter(typeof(PhysicsWorldCreationSystem))]
	partial struct PhysicsBodyCreationSystem : ISystem
	{
		private EntityQuery setupEntitiesQuery;

		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			setupEntitiesQuery = SystemAPI.QueryBuilder().WithAll<PhysicsBodySetup>().WithDisabled<PhysicsBodyState>().WithAll<LocalTransform>().Build();
			state.RequireForUpdate(setupEntitiesQuery);
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			if (setupEntitiesQuery.CalculateEntityCount() == 0)
				return;

			var physicsWorldConfig = SystemAPI.GetSingleton<PhysicsWorldState>();

			var entities = setupEntitiesQuery.ToEntityArray(Allocator.Temp);
			var setups = setupEntitiesQuery.ToComponentDataArray<PhysicsBodySetup>(Allocator.Temp);
			var states = setupEntitiesQuery.ToComponentDataArray<PhysicsBodyState>(Allocator.Temp);
			var transforms = setupEntitiesQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
			var definitionBuffer = new NativeArray<PhysicsBodyDefinition>(entities.Length, Allocator.Temp);

			for(int i = 0; i < entities.Length; i++)
			{
				var definition = setups[i].bodyDefinition;
				definition.position = transforms[i].Position.To2D();
				definitionBuffer[i] = definition;
			}
			
			var physicBodies = PhysicsBody.CreateBatch(physicsWorldConfig.physicsWorld, definitionBuffer, Allocator.Temp);
			
			for(int i = 0; i < entities.Length; i++)
			{
				PhysicsBodyLinkUtility.SetEntity(physicBodies[i], entities[i]);
				var physicsState = states[i];
				physicsState.physicsBody = physicBodies[i];
				state.EntityManager.SetComponentData(entities[i], physicsState);
				state.EntityManager.SetComponentEnabled<PhysicsBodyState>(entities[i], true);
			}
		}
	}

	public static class EntityExtensions
	{
		public static PhysicsMask EncodeAsPhysicsMask(this Entity entity) => UnsafeUtility.As<Entity, PhysicsMask>(ref entity);
	}

	public static class PhysicsMaskExtensions
	{
		public static Entity DecodeFromPhysicsMask(this PhysicsMask physicsMask) => UnsafeUtility.As<PhysicsMask, Entity>(ref physicsMask);
	}

	public static class PhysicsBodyLinkUtility
	{
		public static void SetEntity(PhysicsBody physicsBody, Entity entity) => physicsBody.userData = new PhysicsUserData { physicsMaskValue = entity.EncodeAsPhysicsMask() };
		
		public static Entity GetEntity(PhysicsBody physicsBody) => physicsBody.userData.physicsMaskValue.DecodeFromPhysicsMask();
	}
}