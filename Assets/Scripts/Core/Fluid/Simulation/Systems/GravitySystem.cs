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
	[UpdateAfter(typeof(DensityComputationSystem))]
	public partial struct GravitySystem : ISystem
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
			ref var densityComputationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<DensityComputationSystem>();
			if (fluidCoordinatorSystem.fluidCount == 0)
				return;

			var fluidSimulationConfig = SystemAPI.GetSingleton<FluidSimulationConfig>();

			var applyGravityForcesJob = new ApplyGravityForcesJob
			{
				velocities = spatialCoordinatorSystem.velocityBuffer,
				deltaTime = SystemAPI.Time.DeltaTime,
				gravity = fluidSimulationConfig.gravity
			};
			handle = applyGravityForcesJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, densityComputationSystem.handle);
			state.Dependency = handle;
		}
	}
}