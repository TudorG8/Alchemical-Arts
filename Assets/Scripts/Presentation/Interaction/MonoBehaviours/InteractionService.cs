using AlchemicalArts.Presentation.Interaction.Models;
using AlchemicalArts.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Presentation.Interaction.MonoBehaviours
{
	public class InteractionService : MonoBehaviour
	{
		public EntityQuery InteractionModeQuery { get; private set; }


		private EntityQuery InteractionModeRWQuery { get; set; }


		public void SetInteractionMode(InteractionMode mode)
		{
			if (!InteractionModeRWQuery.TryGetSingletonRW<InteractionModeState>(out var interactionModeState))
			{
				return;
			}

			interactionModeState.ValueRW.mode = interactionModeState.ValueRO.mode == mode ? InteractionMode.None : mode;
		}


		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
			InteractionModeQuery = queryBuilder
				.WithAll<InteractionModeState>()
				.BuildAndReset(World.DefaultGameObjectInjectionWorld.EntityManager);
			InteractionModeRWQuery = queryBuilder
				.WithAllRW<InteractionModeState>()
				.BuildAndReset(World.DefaultGameObjectInjectionWorld.EntityManager);
		}
	}
}