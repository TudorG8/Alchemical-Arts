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


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state) 
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidBuffersSystem>();
			ref var viscosityForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<ViscosityForceSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var writeVelocityBatchesJob = new WriteVelocityBatchesJob
			{
				batchVelocityBuffer = fluidBuffersSystem.batchVelocityBuffer,
				velocityBuffer = fluidBuffersSystem.velocityBuffer,
			};
			state.Dependency = writeVelocityBatchesJob.ScheduleParallel(viscosityForceSystem.handle);

			var setVelocityBatchesJob = new SetVelocityBatchesJob()
			{
				batchVelocity = fluidBuffersSystem.batchVelocityBuffer,
				count = fluidBuffersSystem.count
			};
			handle = setVelocityBatchesJob.Schedule(state.Dependency);
			handle.Complete();
		}
	}
}