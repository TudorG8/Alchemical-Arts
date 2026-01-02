using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Transforms;
using static UnityEngine.LowLevelPhysics2D.PhysicsEvents;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
	public partial struct PhysicsTransformSyncSystem : ISystem
	{
		private ComponentLookup<LocalTransform> localTransformLookup;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsBodyState>();
			state.RequireForUpdate<PhysicsWorldState>();

			localTransformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly:false);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var physicsWorldConfig = SystemAPI.GetSingleton<PhysicsWorldState>();
			
			var bodyUpdatesLength = physicsWorldConfig.physicsWorld.bodyUpdateEvents.Length;
			var bodyUpdates = new NativeArray<BodyUpdateEvent>(
				bodyUpdatesLength,
				Allocator.TempJob,
				NativeArrayOptions.UninitializedMemory
			);
			physicsWorldConfig.physicsWorld.bodyUpdateEvents.CopyTo(bodyUpdates.AsSpan());
			
			localTransformLookup.Update(ref state);
			var writePhysicsTransformsJob = new WritePhysicsTransformsJob
			{
				bodyUpdateEvents = bodyUpdates,
				localTransformLookup = localTransformLookup
			};

			state.Dependency = writePhysicsTransformsJob.Schedule(bodyUpdatesLength, bodyUpdatesLength / JobsUtility.JobWorkerCount, state.Dependency);
		}
	}
}