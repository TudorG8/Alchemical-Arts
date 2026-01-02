using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Fluid.Interaction.Models;
using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Input.Groups;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PotionCraft.Gameplay.Input.Systems
{
	[UpdateInGroup(typeof(InputSystemGroup))]
	public partial struct FluidDragVisualizationSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<DraggingParticlesModeState>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var draggingParticlesModeStateEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRW<DraggingParticlesModeState>(draggingParticlesModeStateEntity);
			var fluidInputState = SystemAPI.GetComponentRW<FluidInputState>(draggingParticlesModeStateEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingParticlesModeStateEntity);

			var inputDataConfig = SystemAPI.GetSingleton<InputDataConfig>();

			var targetEntity = fluidInputConfig.ValueRO.target;
			var targetEntityLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(targetEntity);
			var spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(fluidInputConfig.ValueRO.target);
			
			targetEntityLocalTransform.ValueRW.Position = inputDataConfig.worldPosition.ToFloat3();
			targetEntityLocalTransform.ValueRW.Scale = fluidInputState.ValueRO.interactionRadius * 2;

			spriteRenderer.enabled = draggingParticlesModeState.ValueRO.mode == DraggingParticlesMode.Idle;
		}
	}
}