using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(ViscosityForceSystem))]
	partial struct VelocityWritebackSystem : ISystem
	{
		public JobHandle handle;


		private NativeArray<BatchVelocity> batchVelocityBuffer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			batchVelocityBuffer = new NativeArray<BatchVelocity>(10000, Allocator.Persistent);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			batchVelocityBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state) 
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<LiquidPositionInitializationSystem>();
			ref var viscositySystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<ViscosityForceSystem>();
			if (populateLiquidPositionsSystem.count == 0)
				return;

			var writeVelocityBatchesJob = new WriteVelocityBatchesJob
			{
				batchVelocityBuffer = batchVelocityBuffer,
				velocityBuffer = populateLiquidPositionsSystem.velocityBuffer,
			};
			state.Dependency = writeVelocityBatchesJob.ScheduleParallel(viscositySystem.handle);

			var prepareVelocityBatchesJob = new PrepareVelocityBatchesJob()
			{
				batchVelocity = batchVelocityBuffer,
				count = populateLiquidPositionsSystem.count
			};
			handle = prepareVelocityBatchesJob.Schedule(state.Dependency);
		}
	}
}