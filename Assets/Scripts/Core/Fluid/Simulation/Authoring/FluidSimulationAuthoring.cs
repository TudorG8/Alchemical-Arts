using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Simulation.Authoring
{
	public class FluidSimulationAuthoring : MonoBehaviour
	{
		public FluidSimulationConfig simulationState;
	}

	public class FluidSimulationAuthoringBaker : Baker<FluidSimulationAuthoring>
	{
		public override void Bake(FluidSimulationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, authoring.simulationState);
			AddComponent(entity, new FluidSimulationConstantsConfig());
			AddComponent(entity, new FluidSimulationConstantsConfigInitializeTag());
		}
	}
}