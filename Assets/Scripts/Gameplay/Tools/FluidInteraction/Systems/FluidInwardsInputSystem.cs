using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Groups;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Components;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Models;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Jobs;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.Systems
{
	[UpdateInGroup(typeof(FluidInteractionGroup))]
	public partial struct FluidInwardsInputSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<DraggingParticlesModeState>();
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


			switch(draggingParticlesModeState.ValueRO.mode)
			{
				case DraggingParticlesMode.Idle:
					fluidCoordinatorSystem.inwardsForceBuffer.Clear();
					return;
				
				case DraggingParticlesMode.Inwards:
					var handle = fluidCoordinatorSystem.handle;
					if (fluidCoordinatorSystem.inwardsForceBuffer.Length == 0)
					{
						var collectAffectedParticlesJob = new CollectAffectedParticlesJob
						{
							output = fluidCoordinatorSystem.inwardsForceBuffer.AsParallelWriter(),
							positions = spatialCoordinatorSystem.positionBuffer,
							fluidInputState = fluidInputState.ValueRO,
						};
						handle = collectAffectedParticlesJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, handle);
					}
					else
					{
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
					}

					state.Dependency = handle;
					fluidCoordinatorSystem.RegisterNewHandle(handle);
					break;
			}
		}
	}
}