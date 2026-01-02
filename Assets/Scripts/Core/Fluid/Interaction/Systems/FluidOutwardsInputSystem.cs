using PotionCraft.Core.Input.Components;
using Unity.Burst;
using Unity.Entities;
using PotionCraft.Core.Fluid.Interaction.Groups;
using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Fluid.Interaction.Models;
using PotionCraft.Core.Fluid.Interaction.Jobs;
using PotionCraft.Core.SpatialPartioning.Systems;

namespace PotionCraft.Core.Fluid.Interaction.Systems
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
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			if (fluidBuffersSystem.count == 0)
				return;
			
			var draggingParticlesModeStateEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRO<DraggingParticlesModeState>(draggingParticlesModeStateEntity);
			var fluidInputState = SystemAPI.GetComponentRO<FluidInputState>(draggingParticlesModeStateEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingParticlesModeStateEntity);

			if (draggingParticlesModeState.ValueRO.mode != DraggingParticlesMode.Outwards)
				return;

			var applyOutwardsForcesJob = new ApplyOutwardsForcesJob
			{
				velocities = fluidBuffersSystem.velocityBuffer,
				positions = fluidBuffersSystem.positionBuffer,
				fluidInputConfig = fluidInputConfig.ValueRO,
				fluidInputState = fluidInputState.ValueRO,
				deltaTime = SystemAPI.Time.DeltaTime,
			};
			state.Dependency = applyOutwardsForcesJob.ScheduleParallel(state.Dependency);
			state.Dependency.Complete();
		}
	}
}