using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyGravityForcesJob : IJobEntity
	{
		public NativeArray<float2> velocities;
		
		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public float gravity;


		public void Execute(
			[EntityIndexInQuery] int index)
		{
			velocities[index] -= new float2(0, gravity) * deltaTime;
		}
	}
}