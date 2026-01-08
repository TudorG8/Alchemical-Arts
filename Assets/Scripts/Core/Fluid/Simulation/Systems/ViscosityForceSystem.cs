using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
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
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var pressureForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PressureForceSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			// var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			// var spatialPartioningConstantsConfig = SystemAPI.GetSingleton<SpatialPartioningConstantsConfig>();
			// var fluidSimulationConfig = SystemAPI.GetSingleton<FluidSimulationConfig>();
			// var fluidSimulationConstantsConfig = SystemAPI.GetSingleton<FluidSimulationConstantsConfig>();

			// var applyViscosityForcesJob = new ApplyViscosityForcesJob()
			// {
			// 	velocities = fluidBuffersSystem.velocityBuffer,
			// 	spatial = fluidBuffersSystem.fluidSpatialBuffer.AsArray(),
			// 	spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
			// 	predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
			// 	numParticles = fluidBuffersSystem.fluidCount,
			// 	spatialPartioningConfig = spatialPartioningConfig,
			// 	spatialPartioningConstantsConfig = spatialPartioningConstantsConfig,
			// 	fluidSimulationConfig = fluidSimulationConfig,
			// 	fluidSimulationConstantsConfig = fluidSimulationConstantsConfig,
			// 	deltaTime = SystemAPI.Time.DeltaTime,
			// 	hashingLimit = fluidBuffersSystem.hashingLimit,
			// };
			// handle = applyViscosityForcesJob.ScheduleParallel(pressureForceSystem.handle);
			// handle.Complete();
			handle = pressureForceSystem.handle;
		}
	}
}