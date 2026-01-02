
using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Interaction.Models;
using AlchemicalArts.Core.Input.Components;
using AlchemicalArts.Core.Input.Groups;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Input.Systems
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

			var draggingParticlesModeStateEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRW<DraggingParticlesModeState>(draggingParticlesModeStateEntity);
			var fluidInputConfig = SystemAPI.GetComponentRO<FluidInputConfig>(draggingParticlesModeStateEntity);
			var fluidInputState = SystemAPI.GetComponentRW<FluidInputState>(draggingParticlesModeStateEntity);
			
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