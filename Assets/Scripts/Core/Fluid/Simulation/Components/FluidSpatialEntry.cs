using System.Collections.Generic;
using AlchemicalArts.Core.SpatialPartioning.Components;

namespace AlchemicalArts.Core.Fluid.Simulation.Components
{
	public struct FluidSpatialEntry : ISpatialEntry, ISpatialEntryIndexer
	{
		public int key;

		public int simulationIndex;

		public int fluidIndex;


		public int Key { get => key; set => key = value; }
		
		public int SimulationIndex { get => simulationIndex; set => simulationIndex = value; }
		
		public int Index { get => fluidIndex; set => fluidIndex = value; }
	}

	public struct FluidSpatialEntryComparer : IComparer<FluidSpatialEntry>
	{
		public readonly int Compare(FluidSpatialEntry a, FluidSpatialEntry b)
			=> a.key.CompareTo(b.key);
	}
}