using AlchemicalArts.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct CopyFluidVelocityJob : IJobEntity
	{ 
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocityBufferOutput;

		[ReadOnly]
		public NativeArray<float2> velocityBufferInput;


		public void Execute(
			[EntityIndexInQuery] int i,
			in SpatiallyPartionedIndex spatiallyPartionedItemState,
			in FluidPartionedIndex fluidItemTag)
		{
			velocityBufferOutput[fluidItemTag.index] = velocityBufferInput[spatiallyPartionedItemState.index];
		}
	}
}