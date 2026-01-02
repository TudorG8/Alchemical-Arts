using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAbsent(typeof(PhysicsBodyState))]
	public partial struct PredictPositionsJob : IJobEntity
	{
		[WriteOnly]
		public NativeArray<float2> predictedPositions;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public NativeArray<float2> velocities;
		
		[ReadOnly]
		public float predictionFactor;
		

		public void Execute(
			[EntityIndexInQuery] int index)
		{
			predictedPositions[index] = positions[index] + velocities[index] * predictionFactor;
		}
	}
}