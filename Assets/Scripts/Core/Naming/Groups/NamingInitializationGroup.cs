using Unity.Entities;

namespace AlchemicalArts.Core.Naming.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(WorldUpdateAllocatorResetSystem))]
	public partial class NamingInitializationGroup : ComponentSystemGroup { }
}