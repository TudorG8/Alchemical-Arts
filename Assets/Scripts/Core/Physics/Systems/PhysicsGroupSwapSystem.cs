using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.Physics.Groups;
using Unity.Entities;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsInitializationGroup), OrderFirst = true)]
partial struct PhysicsGroupSwapSystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		var hasWorldState = SystemAPI.HasSingleton<PhysicsWorldState>();
		var physicsSystemGroup = state.World.GetExistingSystemManaged<PhysicsSystemGroup>();

		if (hasWorldState && physicsSystemGroup.Enabled)
		{
			physicsSystemGroup.Enabled = false;
		}
		else if (!hasWorldState && !physicsSystemGroup.Enabled)
		{
			physicsSystemGroup.Enabled = true;
		}
	}
}