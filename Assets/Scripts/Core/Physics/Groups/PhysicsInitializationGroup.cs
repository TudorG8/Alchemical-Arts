using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Physics.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(WorldUpdateAllocatorResetSystem))]
	public partial class PhysicsInitializationGroup : ComponentSystemGroup { }
}