using System.Collections.Generic;

namespace AlchemicalArts.Core.Fluid.Simulation.Components
{
	public struct FluidSpatialEntry
	{
		public int key;

		public int simulationIndex;

		public int fluidIndex;
	}

	public struct FluidSpatialEntryComparer : IComparer<FluidSpatialEntry>
	{
		public readonly int Compare(FluidSpatialEntry a, FluidSpatialEntry b)
			=> a.key.CompareTo(b.key);
	}
}