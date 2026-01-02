using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Simulation.Authoring
{
	public class SimulationAuthoring : MonoBehaviour
	{
		public SimulationConfig simulationState;
	}

	public class SimulationAuthoringBaker : Baker<SimulationAuthoring>
	{
		public override void Bake(SimulationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, authoring.simulationState);
			AddComponent(entity, new SimulationConstantsConfig());
			AddComponent(entity, new SimulationConstantsConfigInitializeTag());
		}
	}
}