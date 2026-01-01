using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(DensityComputationSystem))]
	partial struct GravitySystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var densityComputationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<DensityComputationSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationState = SystemAPI.GetSingleton<SimulationState>();

			var applyGravityForcesJob = new ApplyGravityForcesJob
			{
				velocities = fluidPositionInitializationSystem.velocityBuffer,
				deltaTime = SystemAPI.Time.DeltaTime,
				gravity = simulationState.gravity
			};
			handle = applyGravityForcesJob.ScheduleParallel(densityComputationSystem.handle);
		}
	}
}