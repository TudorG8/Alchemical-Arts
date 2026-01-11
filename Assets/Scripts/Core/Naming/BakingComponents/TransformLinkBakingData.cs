using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.Naming.BakingComponents
{
	[BakingType]
	public struct TransformLinkBakingData : IBufferElementData
	{
		public Entity Parent;

		public Entity Child;

		public bool applyName;

		public FixedString64Bytes Name;
	}
}