using AlchemicalArts.Presentation.Interaction.Models;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Presentation.Interaction.Authoring
{
	public class InteractionModeAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public InteractionMode Mode { get; private set; }
	}

	public class InteractionModeAuthoringBaker : Baker<InteractionModeAuthoring>
	{
		public override void Bake(InteractionModeAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new InteractionModeState()
			{
				mode = authoring.Mode
			});
		}
	}
}