using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidWritebackGroup))]
	public partial struct VelocityWritebackSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			state.RequireForUpdate<FluidSimulationConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state) 
		{
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var fluidCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidCoordinatorSystem>();
			ref var viscosityForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<ViscosityForceSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;


			var readVelocityBatchesJob = new ReadVelocityBatchesJob
			{
				batchVelocityBuffer = fluidCoordinatorSystem.batchVelocityBuffer,
				velocityBuffer = spatialCoordinatorSystem.velocityBuffer,
			};
			var readVelocityBatchesHandle = readVelocityBatchesJob.ScheduleParallel(spatialCoordinatorSystem.simulatedQuery, viscosityForceSystem.handle);


			var setVelocityBatchesJob = new SetVelocityBatchesJob
			{
				batchVelocity = fluidCoordinatorSystem.batchVelocityBuffer,
				count = spatialCoordinatorSystem.count
			};
			handle = setVelocityBatchesJob.Schedule(readVelocityBatchesHandle);
			handle.Complete();
		}
	}
}