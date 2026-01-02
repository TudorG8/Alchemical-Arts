using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Models;
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
	public partial struct FluidDragVisualizationSystem : ISystem
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
			var fluidInputState = SystemAPI.GetComponentRW<FluidInputState>(draggingModeEntity);

			var inputData = SystemAPI.GetSingleton<InputDataConfig>();

			var targetEntity = fluidInputState.ValueRO.target;
			var targetEntityLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(targetEntity);
			var spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(fluidInputState.ValueRO.target);
			
			targetEntityLocalTransform.ValueRW.Position = inputData.worldPosition.ToFloat3();
			targetEntityLocalTransform.ValueRW.Scale = fluidInputState.ValueRO.interactionRadius * 2;

			spriteRenderer.enabled = draggingParticlesModeState.ValueRO.mode == DraggingParticlesMode.Idle;
		}
	}
}