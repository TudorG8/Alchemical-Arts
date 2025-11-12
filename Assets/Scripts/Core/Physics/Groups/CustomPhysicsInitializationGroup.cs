using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Physics.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public partial class CustomPhysicsInitializationGroup : ComponentSystemGroup { }
}