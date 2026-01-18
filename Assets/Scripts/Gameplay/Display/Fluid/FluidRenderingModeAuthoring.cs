using AlchemicalArts.Gameplay.Display.Fluid.Components;
using AlchemicalArts.Gameplay.Display.Fluid.Models;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Display.Fluid.Authoring
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