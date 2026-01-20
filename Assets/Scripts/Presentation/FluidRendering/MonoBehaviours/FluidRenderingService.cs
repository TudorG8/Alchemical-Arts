using AlchemicalArts.Presentation.FluidRendering.Components;
using AlchemicalArts.Presentation.FluidRendering.Models;
using AlchemicalArts.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Presentation.FluidRendering.MonoBehaviours
{
	public class FluidRenderingService : MonoBehaviour
	{
		public EntityQuery FluidRenderingQuery { get; private set; }


		private EntityQuery FluidRenderingQueryRW { get; set; }


		public void SetRenderingMode(FluidRenderingMode fluidRenderingMode)
		{
			if (!FluidRenderingQueryRW.TryGetSingletonRW<FluidRenderingState>(out var fluidRenderingState))
			{
				return;
			}

			fluidRenderingState.ValueRW.mode = fluidRenderingMode;
		}


		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
			FluidRenderingQuery = queryBuilder
				.WithAll<FluidRenderingState>()
				.BuildAndReset(World.DefaultGameObjectInjectionWorld.EntityManager);
			FluidRenderingQueryRW = queryBuilder
				.WithAllRW<FluidRenderingState>()
				.BuildAndReset(World.DefaultGameObjectInjectionWorld.EntityManager);
		}
	}
}
