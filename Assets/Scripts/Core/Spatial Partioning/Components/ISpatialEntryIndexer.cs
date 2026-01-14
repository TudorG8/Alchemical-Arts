namespace AlchemicalArts.Core.SpatialPartioning.Components
{
	public interface ISpatialEntryIndexer
	{
		public int Key { get; set; }

		public int SimulationIndex { get; set; }

		public int Index { get; set; }
	}
}
