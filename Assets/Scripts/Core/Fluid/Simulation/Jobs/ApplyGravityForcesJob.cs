using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct ApplyGravityForcesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocities;
		
		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public float gravity;


		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedItemState)
		{
			velocities[spatiallyPartionedItemState.index] -= new float2(0, gravity) * deltaTime;
		}
	}
}