using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct BuildFluidSpatialEntriesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<FluidSpatialEntry> fluidSpatialBuffer;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> fluidSpatialOffsetsBuffer;

		[ReadOnly]
		public NativeArray<float2> predictedPositionsBuffer;
		
		[ReadOnly]
		public float radius;

		[ReadOnly]
		public int count;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedItemState,
			in FluidPartionedIndex fluidPartionedIndex)
		{
			var cell = SpatialHashingUtility.GetCell2D(predictedPositionsBuffer[spatiallyPartionedItemState.index], radius);
			var hash = SpatialHashingUtility.HashCell2D(cell);
			var cellKey = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
			
			fluidSpatialBuffer[fluidPartionedIndex.index] = new FluidSpatialEntry() { simulationIndex = spatiallyPartionedItemState.index, fluidIndex = fluidPartionedIndex.index, key = cellKey };
			fluidSpatialOffsetsBuffer[fluidPartionedIndex.index] = int.MaxValue;
		}
	}
}