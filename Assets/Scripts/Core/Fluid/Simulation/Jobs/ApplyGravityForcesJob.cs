using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidTag))]
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