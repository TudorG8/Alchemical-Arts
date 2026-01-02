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
	[UpdateAfter(typeof(DensityComputationSystem))]
	public partial struct GravitySystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var densityComputationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<DensityComputationSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var fluidSimulationConfig = SystemAPI.GetSingleton<FluidSimulationConfig>();

			var applyGravityForcesJob = new ApplyGravityForcesJob
			{
				velocities = fluidBuffersSystem.velocityBuffer,
				deltaTime = SystemAPI.Time.DeltaTime,
				gravity = fluidSimulationConfig.gravity
			};
			handle = applyGravityForcesJob.ScheduleParallel(densityComputationSystem.handle);
		}
	}
}