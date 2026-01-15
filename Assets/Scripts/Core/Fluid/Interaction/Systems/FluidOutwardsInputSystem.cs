using AlchemicalArts.Core.Input.Components;
using Unity.Burst;
using Unity.Entities;
using AlchemicalArts.Core.Fluid.Interaction.Groups;
using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Interaction.Models;
using AlchemicalArts.Core.Fluid.Interaction.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;

namespace AlchemicalArts.Core.Fluid.Interaction.Systems
{
	[UpdateInGroup(typeof(FluidInteractionGroup))]
	public partial struct FluidOutwardsInputSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<InputDataConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var fluidCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidCoordinatorSystem>();
			if (fluidCoordinatorSystem.fluidCount == 0)
				return;
			
			var draggingParticlesModeStateEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRO<DraggingParticlesModeState>(draggingParticlesModeStateEntity);
			var fluidInputState = SystemAPI.GetComponentRO<FluidInputState>(draggingParticlesModeStateEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingParticlesModeStateEntity);

			if (draggingParticlesModeState.ValueRO.mode != DraggingParticlesMode.Outwards)
				return;

			var applyOutwardsForcesJob = new ApplyOutwardsForcesJob
			{
				velocities = spatialCoordinatorSystem.velocityBuffer,
				positions = spatialCoordinatorSystem.positionBuffer,
				fluidInputConfig = fluidInputConfig.ValueRO,
				fluidInputState = fluidInputState.ValueRO,
				deltaTime = SystemAPI.Time.DeltaTime,
			};
			var applyOutwardsForcesHandle = applyOutwardsForcesJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, fluidCoordinatorSystem.handle);
			state.Dependency = applyOutwardsForcesHandle;
			fluidCoordinatorSystem.RegisterNewHandle(applyOutwardsForcesHandle);
		}
	}
}