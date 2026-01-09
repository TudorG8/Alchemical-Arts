using AlchemicalArts.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct CopyFluidPositionJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> positionsBufferOutput;

		[ReadOnly]
		public NativeArray<float2> positionsBufferInput;


		public void Execute(
			[EntityIndexInQuery] int i,
			in SpatiallyPartionedIndex spatiallyPartionedItemState,
			in FluidPartionedIndex fluidItemTag)
		{
			positionsBufferOutput[fluidItemTag.index] = positionsBufferInput[spatiallyPartionedItemState.index];
		}
	}
}