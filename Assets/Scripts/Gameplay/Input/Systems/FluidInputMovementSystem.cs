
using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Fluid.Interaction.Models;
using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Input.Groups;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;

namespace PotionCraft.Gameplay.Input.Systems
{
	[UpdateInGroup(typeof(InputSystemGroup), OrderLast = true)]
	public partial struct FluidInputMovementSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<DraggingParticlesModeState>();
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// TODO
			// if mode is not active
			// 	return

			var draggingModeEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var fluidInputConfig = SystemAPI.GetComponentRO<FluidInputConfig>(draggingModeEntity);
			var fluidInputState = SystemAPI.GetComponentRW<FluidInputState>(draggingModeEntity);
			var draggingParticlesModeState = SystemAPI.GetComponentRW<DraggingParticlesModeState>(draggingModeEntity);
			
			var inputData = SystemAPI.GetSingleton<InputDataConfig>();

			switch (draggingParticlesModeState.ValueRO.mode)
			{
				case DraggingParticlesMode.Idle when inputData.primaryPressed: draggingParticlesModeState.ValueRW.mode = DraggingParticlesMode.Inwards; break;
				case DraggingParticlesMode.Idle when inputData.secondaryPressed: draggingParticlesModeState.ValueRW.mode = DraggingParticlesMode.Outwards; break;
				case DraggingParticlesMode.Inwards when !inputData.primaryPressed: draggingParticlesModeState.ValueRW.mode = DraggingParticlesMode.Idle; break;
				case DraggingParticlesMode.Outwards when !inputData.secondaryPressed: draggingParticlesModeState.ValueRW.mode = DraggingParticlesMode.Idle; break;
			}

			fluidInputState.ValueRW.position = inputData.worldPosition;
			
			if (draggingParticlesModeState.ValueRW.mode == DraggingParticlesMode.Idle)
			{
				fluidInputState.ValueRW.interactionRadius = fluidInputConfig.ValueRO.interactionRadiusBounds
					.Clamp(fluidInputState.ValueRW.interactionRadius + inputData.scrollDelta * fluidInputConfig.ValueRO.scrollSpeed * SystemAPI.Time.DeltaTime);
				return;
			}
		}
	}
}