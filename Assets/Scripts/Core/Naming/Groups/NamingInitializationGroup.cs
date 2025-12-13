using Unity.Entities;

namespace PotionCraft.Core.Naming.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(WorldUpdateAllocatorResetSystem))]
	public partial class NamingInitializationGroup : ComponentSystemGroup { }
}