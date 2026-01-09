using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct PredictPositionsJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> predictedPositions;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public NativeArray<float2> velocities;
		
		[ReadOnly]
		public float predictionFactor;
		

		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedIndex)
		{
			predictedPositions[spatiallyPartionedIndex.index] = positions[spatiallyPartionedIndex.index] + velocities[spatiallyPartionedIndex.index] * predictionFactor;
		}
	}
}