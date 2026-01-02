using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Naming.Components
{
	public struct EntityNameConfig : IComponentData, IEnableableComponent
	{
		public FixedString64Bytes value;
	}
}