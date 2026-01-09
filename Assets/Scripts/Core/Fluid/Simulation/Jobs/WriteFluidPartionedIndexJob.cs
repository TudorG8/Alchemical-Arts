using AlchemicalArts.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct WriteFluidPartionedIndexJob : IJobEntity
	{
		public void Execute(
			[EntityIndexInQuery] int index,
			ref FluidPartionedIndex fluidPartionedIndex)
		{
			fluidPartionedIndex.index = index;
		}
	}
}