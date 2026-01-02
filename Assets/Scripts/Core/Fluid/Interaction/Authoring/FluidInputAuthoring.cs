using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Fluid.Interaction.Models;
using PotionCraft.Shared.Models;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Interaction.Authoring
{
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
			
			AddComponent(entity, new FluidInputState()
			{
				interactionRadius = authoring.interactionRadius.value,
				target = GetEntity(authoring.target, TransformUsageFlags.Dynamic)
			});

			AddComponent(entity, new FluidInputConfig()
			{
				interactionStrength = authoring.interactionStrength,
				interactionRadiusBounds = authoring.interactionRadius.bounds,
				damping = authoring.damping,
				scrollSpeed = authoring.scrollSpeed,
			});
		}
	}
}