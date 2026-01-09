using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidWritebackGroup), OrderLast = true)]
	public partial struct PhysicsIntegrationSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var worldQuerry = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldState>().Build(state.EntityManager);
			var world = worldQuerry.GetSingletonEntity();
			var worldScript = state.EntityManager.GetComponentData<PhysicsWorldState>(world);
			worldScript.physicsWorld.Simulate(SystemAPI.Time.DeltaTime);
		}
	}
}