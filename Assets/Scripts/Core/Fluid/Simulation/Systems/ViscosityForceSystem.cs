using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using PotionCraft.Core.SpatialPartioning.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
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
		}


		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var pressureForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PressureForceSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			var spatialPartioningConstantsConfig = SystemAPI.GetSingleton<SpatialPartioningConstantsConfig>();
			var fluidSimulationConfig = SystemAPI.GetSingleton<FluidSimulationConfig>();
			var fluidSimulationConstantsConfig = SystemAPI.GetSingleton<FluidSimulationConstantsConfig>();

			var applyViscosityForcesJob = new ApplyViscosityForcesJob()
			{
				velocities = fluidBuffersSystem.velocityBuffer,
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
				predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
				numParticles = fluidBuffersSystem.count,
				spatialPartioningConfig = spatialPartioningConfig,
				spatialPartioningConstantsConfig = spatialPartioningConstantsConfig,
				fluidSimulationConfig = fluidSimulationConfig,
				fluidSimulationConstantsConfig = fluidSimulationConstantsConfig,
				deltaTime = SystemAPI.Time.DeltaTime,
				hashingLimit = fluidBuffersSystem.hashingLimit,
			};
			handle = applyViscosityForcesJob.ScheduleParallel(pressureForceSystem.handle);
		}
	}
}