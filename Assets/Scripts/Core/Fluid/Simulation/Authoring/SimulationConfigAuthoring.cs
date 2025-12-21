using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Simulation.Authoring
{
	public class SimulationConfigAuthoring : MonoBehaviour
	{
		public SimulationConfig simulationConfig;
	}

	public class SimulationConfigBaker : Baker<SimulationConfigAuthoring>
	{
		public override void Bake(SimulationConfigAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, authoring.simulationConfig);
		}
	}
}