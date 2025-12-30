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
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
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
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var viscosityForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<ViscosityForceSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var writeVelocityBatchesJob = new WriteVelocityBatchesJob
			{
				batchVelocityBuffer = batchVelocityBuffer,
				velocityBuffer = fluidPositionInitializationSystem.velocityBuffer,
			};
			state.Dependency = writeVelocityBatchesJob.ScheduleParallel(viscosityForceSystem.handle);

			var prepareVelocityBatchesJob = new PrepareVelocityBatchesJob()
			{
				batchVelocity = batchVelocityBuffer,
				count = fluidPositionInitializationSystem.count
			};
			handle = prepareVelocityBatchesJob.Schedule(state.Dependency);
		}
	}
}