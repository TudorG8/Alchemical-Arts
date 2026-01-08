using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyGravityForcesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<FluidSpatialEntry> spatial;
		
		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public float gravity;


		public void Execute(
			[EntityIndexInQuery] int i,
			in SpatiallyPartionedItemState spatiallyPartionedItemState)
		{
			// var simulationIndex = spatial[i].simulationIndex;
			var simulationIndex = spatiallyPartionedItemState.index;
			velocities[simulationIndex] -= new float2(0, gravity) * deltaTime;
		}
	}
}