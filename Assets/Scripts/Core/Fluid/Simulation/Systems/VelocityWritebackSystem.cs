using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(ViscosityForceSystem))]
	public partial struct VelocityWritebackSystem : ISystem
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
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var viscosityForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<ViscosityForceSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var readVelocityBatchesJob = new ReadVelocityBatchesJob
			{
				batchVelocityBuffer = fluidBuffersSystem.batchVelocityBuffer,
				velocityBuffer = fluidBuffersSystem.velocityBuffer,
			};
			state.Dependency = readVelocityBatchesJob.ScheduleParallel(viscosityForceSystem.handle);

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