using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Simulation.Authoring
{
	public class SpatialPartioningAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public SpatialPartioningConfig SimulationState { get; private set; }
	}

	public class SpatialPartioningAuthoringBaker : Baker<SpatialPartioningAuthoring>
	{
		public override void Bake(SpatialPartioningAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, authoring.SimulationState);

			var list = new FixedList128Bytes<int2> ();
			for (int y = -1; y <= 1; y++)
			{
				for (int x = -1; x <= 1; x++)
				{
					list.Add(new int2(x, y));
				}
			}
			AddComponent(entity, new SpatialPartioningConstantsConfig() { offsets = list });
		}
	}
}