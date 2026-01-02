using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.Naming.Components
{
	public struct EntityNameConfig : IComponentData, IEnableableComponent
	{
		public FixedString64Bytes value;
	}
}