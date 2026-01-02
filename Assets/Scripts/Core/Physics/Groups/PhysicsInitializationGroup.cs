using Unity.Entities;
using Unity.Scenes;

namespace AlchemicalArts.Core.Physics.Groups
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(SceneSystemGroup))]
	public partial class PhysicsInitializationGroup : ComponentSystemGroup { }
}