using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Fluid.Interaction.Models;
using PotionCraft.Shared.Models;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Interaction.Authoring
{
	public class FluidInputAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public float InteractionStrength { get; private set; }

		[field: SerializeField]
		public MinMaxFloatValueWrapper InteractionRadius { get; private set; }

		[field: SerializeField]
		public float Damping { get; private set; }

		[field: SerializeField]
		public float ScrollSpeed { get; private set; }

		[field: SerializeField]
		public Transform Target { get; private set; }
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
				interactionRadius = authoring.InteractionRadius.value,
			});

			AddComponent(entity, new FluidInputConfig()
			{
				interactionStrength = authoring.InteractionStrength,
				interactionRadiusBounds = authoring.InteractionRadius.bounds,
				damping = authoring.Damping,
				scrollSpeed = authoring.ScrollSpeed,
				target = GetEntity(authoring.Target, TransformUsageFlags.Dynamic)
			});
		}
	}
}