using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Simulation.Authoring
{
	public class FluidAuthoring : MonoBehaviour
	{
		
	}

	public class FluidAuthoringBaker : Baker<FluidAuthoring>
	{
		public override void Bake(FluidAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FluidPartionedIndex());
		}
	}
}