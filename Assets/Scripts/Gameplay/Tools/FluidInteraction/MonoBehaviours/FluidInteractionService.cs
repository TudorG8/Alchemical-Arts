using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Interaction.Models;
using AlchemicalArts.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.MonoBehaviours
{
	public class FluidInteractionService : MonoBehaviour
	{
		public EntityQuery FluidInputQuery { get; private set; }


		private EntityQuery FluidInputRWQuery { get; set;}


		public void SetInteractionRadiusToPercentage(float percentage)
		{
			if (!FluidInputRWQuery.TryGetSingletonRW<FluidInputState>(out var fluidInputState) ||
				!FluidInputRWQuery.TryGetSingleton<FluidInputConfig>(out var fluidInputConfig))
			{
				return;
			}

			fluidInputState.ValueRW.interactionRadius = fluidInputConfig.interactionRadiusBounds.PercentageValue(percentage);
		}

		public void SetInteractionMode(DraggingParticlesMode mode)
		{
			if (World.DefaultGameObjectInjectionWorld == null || !FluidInputRWQuery.TryGetSingletonRW<DraggingParticlesModeState>(out var draggingParticlesModeState))
			{
				return;
			}

			draggingParticlesModeState.ValueRW.mode = mode;
		}

		public void SetInteractionPosition(float2 position)
		{
			if (!FluidInputRWQuery.TryGetSingletonRW<FluidInputState>(out var fluidInputState))
			{
				return;
			}

			fluidInputState.ValueRW.position = position;
		}

		public void ApplyScrollDelta(float scrollDelta)
		{
			if (!FluidInputRWQuery.TryGetSingletonRW<FluidInputState>(out var fluidInputState) ||
				!FluidInputRWQuery.TryGetSingleton<FluidInputConfig>(out var fluidInputConfig))
			{
				return;
			}

			fluidInputState.ValueRW.interactionRadius = fluidInputConfig.interactionRadiusBounds
				.Clamp(fluidInputState.ValueRW.interactionRadius + scrollDelta * fluidInputConfig.scrollSpeed * Time.deltaTime);
		}


		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
			FluidInputRWQuery = queryBuilder
				.WithAllRW<DraggingParticlesModeState>()
				.WithAllRW<FluidInputState>()
				.WithAll<FluidInputConfig>()
				.BuildAndReset(World.DefaultGameObjectInjectionWorld.EntityManager);
			FluidInputQuery = queryBuilder
				.WithAll<DraggingParticlesModeState>()
				.WithAll<FluidInputState>()
				.WithAll<FluidInputConfig>()
				.BuildAndReset(World.DefaultGameObjectInjectionWorld.EntityManager);
		}
	}
}