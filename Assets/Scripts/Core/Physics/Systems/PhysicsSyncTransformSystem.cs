using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Extensions;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using static UnityEngine.LowLevelPhysics2D.PhysicsEvents;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
	partial struct PhysicsSyncTransformSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsBodyState>();
			state.RequireForUpdate<PhysicsWorldState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var physicsWorldConfig = SystemAPI.GetSingleton<PhysicsWorldState>();
			
			var bodyUpdatesLength = physicsWorldConfig.physicsWorld.bodyUpdateEvents.Length;
			NativeArray<BodyUpdateEvent> bodyUpdates = new(
				bodyUpdatesLength,
				Allocator.TempJob,
				NativeArrayOptions.UninitializedMemory
			);
			physicsWorldConfig.physicsWorld.bodyUpdateEvents.CopyTo(bodyUpdates.AsSpan());
			
			var physicsTransformWriteJob = new PhysicsTransformWriteJob
			{
				BodyUpdateEvents = bodyUpdates,
				TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
			};

			const int batchCount = 8;
			state.Dependency = physicsTransformWriteJob.Schedule(
				bodyUpdatesLength,
				bodyUpdatesLength / batchCount,
				state.Dependency);
		}

		[BurstCompile]
		public partial struct PhysicsTransformWriteJob : IJobParallelFor
		{
			[ReadOnly]
			[DeallocateOnJobCompletion]
			public NativeArray<BodyUpdateEvent> BodyUpdateEvents;
			
			[NativeDisableParallelForRestriction]
			public ComponentLookup<LocalTransform> TransformLookup;

			public void Execute(int index)
			{
				var bodyUpdate = BodyUpdateEvents[index];
				var entity = bodyUpdate.body.userData.physicsMaskValue.DecodeFromPhysicsMask();
				var transform = TransformLookup[entity];

				var position = bodyUpdate.transform.position.ToFloat3();
				var rotation = bodyUpdate.transform.rotation.ToECSQuaternion();
				transform.Position = position;
				transform.Rotation = rotation;

				TransformLookup[entity] = transform;
			}
		}
	}
}