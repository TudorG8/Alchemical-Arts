
using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Input.Groups;
using Unity.Burst;
using Unity.Entities;

namespace PotionCraft.Gameplay.Input.Systems
{
	[UpdateInGroup(typeof(InputSystemGroup), OrderLast = true)]
	partial struct InputFluidMoverSystem : ISystem
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
			var fluidInputState = SystemAPI.GetComponentRO<FluidInputState>(draggingModeEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingModeEntity);
			var draggingParticlesModeState = SystemAPI.GetComponentRW<DraggingParticlesModeState>(draggingModeEntity);
			
			var inputData = SystemAPI.GetSingleton<InputDataState>();

			switch (draggingParticlesModeState.ValueRO.mode)
			{
				case DraggingParticlesMode.Idle when inputData.primaryPressed: draggingParticlesModeState.ValueRW.mode = DraggingParticlesMode.Inwards; break;
				case DraggingParticlesMode.Inwards when !inputData.primaryPressed: draggingParticlesModeState.ValueRW.mode = DraggingParticlesMode.Idle; break;
			}

			fluidInputConfig.ValueRW.position = inputData.worldPosition;
			
			if (draggingParticlesModeState.ValueRW.mode == DraggingParticlesMode.Idle)
			{
				fluidInputConfig.ValueRW.interactionRadius = fluidInputState.ValueRO.interactionRadiusBounds
					.Clamp(fluidInputConfig.ValueRW.interactionRadius + inputData.scrollDelta * fluidInputState.ValueRO.scrollSpeed * SystemAPI.Time.DeltaTime);
				return;
			}
		}
	}
}