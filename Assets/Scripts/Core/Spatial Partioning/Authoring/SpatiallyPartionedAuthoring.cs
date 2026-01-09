using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Simulation.Authoring
{
	public class SpatiallyPartionedAuthoring : MonoBehaviour
	{
	}

	public class SpatiallyPartionedAuthoringBaker : Baker<SpatiallyPartionedAuthoring>
	{
		public override void Bake(SpatiallyPartionedAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new SpatiallyPartionedIndex());
		}
	}
}