using Unity.Entities;

namespace PotionCraft.Core.Camera.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(WorldUpdateAllocatorResetSystem))]
	public partial class CameraInitializationGroup : ComponentSystemGroup { }
}