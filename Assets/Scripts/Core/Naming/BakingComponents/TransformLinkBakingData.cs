using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Naming.BakingComponents
{
	[BakingType]
	public struct TransformLinkBakingData : IBufferElementData
	{
		public Entity Parent;

		public Entity Child;

		public FixedString64Bytes Name;
	}
}