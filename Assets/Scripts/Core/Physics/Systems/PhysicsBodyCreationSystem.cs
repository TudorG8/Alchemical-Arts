using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.Physics.Extensions;
using AlchemicalArts.Core.Physics.Groups;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	[UpdateAfter(typeof(PhysicsWorldCreationSystem))]
	public partial struct PhysicsBodyCreationSystem : ISystem
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
				physicBodies[i].SetEntity(entities[i]);
				var physicsState = states[i];
				physicsState.physicsBody = physicBodies[i];
				state.EntityManager.SetComponentData(entities[i], physicsState);
				state.EntityManager.SetComponentEnabled<PhysicsBodyState>(entities[i], true);
			}
		}
	}
}