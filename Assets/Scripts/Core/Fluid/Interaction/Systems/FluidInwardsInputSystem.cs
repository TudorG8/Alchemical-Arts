using AlchemicalArts.Core.Input.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using AlchemicalArts.Core.Fluid.Interaction.Groups;
using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Interaction.Models;
using AlchemicalArts.Core.Fluid.Interaction.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;

namespace AlchemicalArts.Core.Fluid.Interaction.Systems
{
	[UpdateInGroup(typeof(FluidInteractionGroup))]
	public partial struct FluidInwardsInputSystem : ISystem
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


			var handle = state.Dependency;
			switch(draggingParticlesModeState.ValueRO.mode)
			{
				case DraggingParticlesMode.Idle:
					fluidCoordinatorSystem.inwardsForceBuffer.Clear();
					break;
				case DraggingParticlesMode.Inwards:
					if (fluidCoordinatorSystem.inwardsForceBuffer.Length == 0)
					{
						var collectAffectedParticlesJob = new CollectAffectedParticlesJob
						{
							output = fluidCoordinatorSystem.inwardsForceBuffer.AsParallelWriter(),
							positions = spatialCoordinatorSystem.positionBuffer,
							fluidInputState = fluidInputState.ValueRO,
						};
						handle = collectAffectedParticlesJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, handle);
						break;
					}
					
					var applyInputToCache = new ApplyInwardsForcesJob
					{
						velocities = spatialCoordinatorSystem.velocityBuffer,
						positions = spatialCoordinatorSystem.positionBuffer,
						fluidInputConfig = fluidInputConfig.ValueRO,
						fluidInputState = fluidInputState.ValueRO,
						deltaTime = SystemAPI.Time.DeltaTime,
						indexes = fluidCoordinatorSystem.inwardsForceBuffer,
					};
					handle = applyInputToCache.Schedule(fluidCoordinatorSystem.inwardsForceBuffer.Length, 64, handle);
					break;
			}

			handle.Complete();
		}
	}
}