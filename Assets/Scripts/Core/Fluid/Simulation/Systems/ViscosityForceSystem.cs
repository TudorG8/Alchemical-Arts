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
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(PressureForceSystem))]
	public partial struct ViscosityForceSystem : ISystem
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
			ref var pressureForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PressureForceSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			var spatialPartioningConstantsConfig = SystemAPI.GetSingleton<SpatialPartioningConstantsConfig>();
			var fluidSimulationConfig = SystemAPI.GetSingleton<FluidSimulationConfig>();
			var fluidSimulationConstantsConfig = SystemAPI.GetSingleton<FluidSimulationConstantsConfig>();


			var applyViscosityForcesJob = new ApplyViscosityForcesJob()
			{
				velocities = spatialCoordinatorSystem.velocityBuffer,
				spatial = fluidCoordinatorSystem.fluidSpatialBuffer,
				spatialOffsets = fluidCoordinatorSystem.fluidSpatialOffsetsBuffer,
				predictedPositions = spatialCoordinatorSystem.predictedPositionsBuffer,
				numParticles = fluidCoordinatorSystem.fluidCount,
				spatialPartioningConfig = spatialPartioningConfig,
				spatialPartioningConstantsConfig = spatialPartioningConstantsConfig,
				fluidSimulationConfig = fluidSimulationConfig,
				fluidSimulationConstantsConfig = fluidSimulationConstantsConfig,
				deltaTime = SystemAPI.Time.DeltaTime,
				hashingLimit = spatialCoordinatorSystem.hashingLimit,
			};
			handle = applyViscosityForcesJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, pressureForceSystem.handle);
			handle.Complete();
		}
	}
}