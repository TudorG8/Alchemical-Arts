using Unity.Entities;

namespace AlchemicalArts.Core.Camera.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(WorldUpdateAllocatorResetSystem))]
	public partial class CameraInitializationGroup : ComponentSystemGroup { }
}