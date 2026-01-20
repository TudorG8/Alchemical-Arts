using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Interaction.Models;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.Systems
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public partial struct FluidInteractionRenderingSystem : ISystem
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

			var targetEntity = fluidInputConfig.ValueRO.target;
			var targetEntityLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(targetEntity);
			var spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(fluidInputConfig.ValueRO.target);
			
			targetEntityLocalTransform.ValueRW.Position = fluidInputState.ValueRO.position.ToFloat3();
			targetEntityLocalTransform.ValueRW.Scale = fluidInputState.ValueRO.interactionRadius * 2;

			spriteRenderer.enabled = draggingParticlesModeState.ValueRO.mode == DraggingParticlesMode.Idle;
		}
	}
}