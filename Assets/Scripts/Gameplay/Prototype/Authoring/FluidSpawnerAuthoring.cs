using AlchemicalArts.Gameplay.Prototype.Components;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Prototype.Authoring
{
	public class FluidSpawnerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public int Max { get; private set; }

		[field: SerializeField]
		public float Delay { get; private set; }

		[field: SerializeField]
		public GameObject Fluid { get; private set; }
	}

	public class FluidSpawnerAuthoringBaker : Baker<FluidSpawnerAuthoring>
	{
		public override void Bake(FluidSpawnerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FluidSpawnerState());
			AddComponent(entity, new FluidSpawnerConfig()
			{
				max = authoring.Max,
				delay = authoring.Delay,
				fluid = GetEntity(authoring.Fluid, TransformUsageFlags.Dynamic)
			});
		}
	}
}