using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Models;
using PotionCraft.Core.Fluid.Simulation.Utility;
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
	public partial struct BuildSpatialEntriesJob : IJobEntity
	{
		[WriteOnly]
		public NativeSlice<SpatialEntry> spatialOutput;

		[WriteOnly]
		public NativeArray<int> spatialOffsetOutput;

		[ReadOnly]
		public NativeArray<float2> predictedPositions;
		
		[ReadOnly]
		public float radius;

		[ReadOnly]
		public int count;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int index)
		{
			var cell = SpatialHashingUtility.GetCell2D(predictedPositions[index], radius);
			var hash = SpatialHashingUtility.HashCell2D(cell);
			var cellKey = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
			
			spatialOutput[index] = new SpatialEntry() { index = index, key = cellKey };
			spatialOffsetOutput[index] = int.MaxValue;
		}
	}
}