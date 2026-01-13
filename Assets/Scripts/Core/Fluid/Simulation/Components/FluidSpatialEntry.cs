using System.Collections.Generic;
using AlchemicalArts.Core.SpatialPartioning.Components;

namespace AlchemicalArts.Core.Fluid.Simulation.Components
{
	public struct FluidSpatialEntry : ISpatialEntry
	{
		public int key;

		public int simulationIndex;

		public int fluidIndex;


		public int Key { get => key; set => key = value; }
	}

	public struct FluidSpatialEntryComparer : IComparer<FluidSpatialEntry>
	{
		public readonly int Compare(FluidSpatialEntry a, FluidSpatialEntry b)
			=> a.key.CompareTo(b.key);
	}
}