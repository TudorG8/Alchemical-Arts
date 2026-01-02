using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Simulation.Authoring
{
	public class SpatialPartioningAuthoring : MonoBehaviour
	{
		public SpatialPartioningConfig simulationState;
	}

	public class SpatialPartioningAuthoringBaker : Baker<SpatialPartioningAuthoring>
	{
		public override void Bake(SpatialPartioningAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, authoring.simulationState);

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