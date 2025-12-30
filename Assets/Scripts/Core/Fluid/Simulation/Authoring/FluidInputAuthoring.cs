using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Simulation.Authoring
{
	[System.Serializable]
	public class MinMaxFloatValueWrapper
	{
		public MinMaxFloatValue bounds;

		public float value;
	}

	public class FluidInputAuthoring : MonoBehaviour
	{
		public float interactionStrength;

		public MinMaxFloatValueWrapper interactionRadius;

		public float damping;

		public float scrollSpeed;

		public Transform target;
	}

	public class FluidInputAuthoringBaker : Baker<FluidInputAuthoring>
	{
		public override void Bake(FluidInputAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new DraggingParticlesModeState()
			{
				mode = DraggingParticlesMode.Idle
			});
			
			AddComponent(entity, new FluidInputConfig()
			{
				interactionRadius = authoring.interactionRadius.value,
				target = GetEntity(authoring.target, TransformUsageFlags.Dynamic)
			});

			AddComponent(entity, new FluidInputState()
			{
				interactionStrength = authoring.interactionStrength,
				interactionRadiusBounds = authoring.interactionRadius.bounds,
				damping = authoring.damping,
				scrollSpeed = authoring.scrollSpeed,
			});
		}
	}
}