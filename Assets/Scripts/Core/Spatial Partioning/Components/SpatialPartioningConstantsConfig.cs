using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.SpatialPartioning.Components
{
	public struct SpatialPartioningConstantsConfig : IComponentData
	{
		public FixedList128Bytes<int2> offsets;
	}
}