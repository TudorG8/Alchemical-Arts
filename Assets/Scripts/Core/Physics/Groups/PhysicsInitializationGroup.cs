using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

namespace PotionCraft.Core.Physics.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(SceneSystemGroup))]
	public partial class PhysicsInitializationGroup : ComponentSystemGroup { }
}