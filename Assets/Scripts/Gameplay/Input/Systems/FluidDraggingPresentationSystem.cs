using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Input.Groups;
using PotionCraft.Gameplay.Input.Systems;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PotionCraft.Gameplay.Input.Systems
{
	[UpdateInGroup(typeof(InputSystemGroup))]
	partial struct FluidDraggingPresentationSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<DraggingParticlesModeState>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var draggingModeEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRW<DraggingParticlesModeState>(draggingModeEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingModeEntity);

			var inputData = SystemAPI.GetSingleton<InputDataState>();

			var targetEntity = fluidInputConfig.ValueRO.target;
			var targetEntityLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(targetEntity);
			var spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(fluidInputConfig.ValueRO.target);
			
			targetEntityLocalTransform.ValueRW.Position = inputData.worldPosition.ToFloat3();
			targetEntityLocalTransform.ValueRW.Scale = fluidInputConfig.ValueRO.interactionRadius * 2;

			spriteRenderer.enabled = draggingParticlesModeState.ValueRO.mode == DraggingParticlesMode.Idle;
		}
	}
}