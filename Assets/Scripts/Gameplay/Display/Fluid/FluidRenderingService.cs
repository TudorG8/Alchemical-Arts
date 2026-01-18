using AlchemicalArts.Gameplay.Display.Fluid.Components;
using AlchemicalArts.Gameplay.Display.Fluid.Models;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Display.Fluid.MonoBehaviours
{
	public class FluidRenderingService : MonoBehaviour
	{
		public void ChangeToMode(FluidRenderingMode fluidRenderingMode)
		{
			var world = World.DefaultGameObjectInjectionWorld;
			var entityManager = world.EntityManager;

			var query = new EntityQueryBuilder(Allocator.Temp).WithAllRW<FluidRenderingState>().Build(entityManager);
			var fluidRenderingState = query.GetSingletonRW<FluidRenderingState>();
			fluidRenderingState.ValueRW.mode = fluidRenderingMode;
		}
	}
}
