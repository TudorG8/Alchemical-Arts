using PotionCraft.Core.Input.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using PotionCraft.Core.Fluid.Interaction.Groups;
using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Fluid.Interaction.Models;
using PotionCraft.Core.Fluid.Interaction.Jobs;
using PotionCraft.Core.SpatialPartioning.Systems;

namespace PotionCraft.Core.Fluid.Interaction.Systems
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
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			if (fluidBuffersSystem.count == 0)
				return;
			
			var draggingModeEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRO<DraggingParticlesModeState>(draggingModeEntity);
			var fluidInputState = SystemAPI.GetComponentRO<FluidInputState>(draggingModeEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingModeEntity);

			var handle = state.Dependency;
			switch(draggingParticlesModeState.ValueRO.mode)
			{
				case DraggingParticlesMode.Idle:
					fluidBuffersSystem.inwardsForceBuffer.Clear();
					break;
				case DraggingParticlesMode.Inwards:
					if (fluidBuffersSystem.inwardsForceBuffer.Length == 0)
					{
						var collectAffectedParticlesJob = new CollectAffectedParticlesJob
						{
							output = fluidBuffersSystem.inwardsForceBuffer.AsParallelWriter(),
							positions = fluidBuffersSystem.positionBuffer,
							fluidInputState = fluidInputState.ValueRO,
						};
						handle = collectAffectedParticlesJob.ScheduleParallel(handle);
						break;
					}
					
					var applyInputToCache = new ApplyInwardsForcesJob
					{
						velocities = fluidBuffersSystem.velocityBuffer,
						positions = fluidBuffersSystem.positionBuffer,
						fluidInputConfig = fluidInputConfig.ValueRO,
						fluidInputState = fluidInputState.ValueRO,
						deltaTime = SystemAPI.Time.DeltaTime,
						indexes = fluidBuffersSystem.inwardsForceBuffer,
					};
					handle = applyInputToCache.Schedule(fluidBuffersSystem.inwardsForceBuffer.Length, 64, handle);
					break;
			}

			handle.Complete();
		}
	}
}