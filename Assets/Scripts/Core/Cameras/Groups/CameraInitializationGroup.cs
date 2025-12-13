using Unity.Entities;

namespace PotionCraft.Core.Cameras.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(WorldUpdateAllocatorResetSystem))]
	public partial class CameraInitializationGroup : ComponentSystemGroup { }
}