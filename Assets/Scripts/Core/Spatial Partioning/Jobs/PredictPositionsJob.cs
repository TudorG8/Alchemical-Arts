using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct PredictPositionsJob : IJobEntity
	{
		[WriteOnly]
		public NativeArray<float2> predictedPositionsBuffer;

		[ReadOnly]
		public NativeArray<float2> positionBuffer;

		[ReadOnly]
		public NativeArray<float2> velocityBuffer;
		
		[ReadOnly]
		public float predictionFactor;
		

		public void Execute(in SpatiallyPartionedItemState spatiallyPartionedItemState)
		{
			predictedPositionsBuffer[spatiallyPartionedItemState.index] = positionBuffer[spatiallyPartionedItemState.index] + velocityBuffer[spatiallyPartionedItemState.index] * predictionFactor;
		}
	}
}