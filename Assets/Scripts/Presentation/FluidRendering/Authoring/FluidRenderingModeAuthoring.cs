using AlchemicalArts.Presentation.FluidRendering.Components;
using AlchemicalArts.Presentation.FluidRendering.Models;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Presentation.FluidRendering.Authoring
{
	public class FluidRenderingModeAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public FluidRenderingMode Mode { get; private set; }
	}

	public class FluidRenderingModeAuthoringBaker : Baker<FluidRenderingModeAuthoring>
	{
		public override void Bake(FluidRenderingModeAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FluidRenderingState() { mode = authoring.Mode});
		}
	}
}