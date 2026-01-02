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
	[UpdateAfter(typeof(GravitySystem))]
	public partial struct PressureForceSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var fluidInwardsInputSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<GravitySystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();
			var simulationConstantsConfig = SystemAPI.GetSingleton<SimulationConstantsConfig>();

			var applyPressureForcesJob = new ApplyPressureForcesJob()
			{
				velocities = fluidBuffersSystem.velocityBuffer,
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
				densities = fluidBuffersSystem.densityBuffer,
				nearDensity = fluidBuffersSystem.nearDensityBuffer,
				predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
				numParticles = fluidBuffersSystem.count,
				simulationConfig = simulationConfig,
				simulationConstantsConfig = simulationConstantsConfig,
				deltaTime = SystemAPI.Time.DeltaTime,
				hashingLimit = fluidBuffersSystem.hashingLimit
			};
			handle = applyPressureForcesJob.ScheduleParallel(fluidInwardsInputSystem.handle);
		}
	}
}