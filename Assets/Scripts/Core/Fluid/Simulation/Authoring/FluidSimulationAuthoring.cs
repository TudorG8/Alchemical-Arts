using AlchemicalArts.Core.Fluid.Simulation.Components;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Simulation.Authoring
{
	public class FluidSimulationAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public FluidSimulationConfig SimulationState { get; private set; }
	}

	public class FluidSimulationAuthoringBaker : Baker<FluidSimulationAuthoring>
	{
		public override void Bake(FluidSimulationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, authoring.SimulationState);
			AddComponent(entity, new FluidSimulationConstantsConfig());
			AddComponent(entity, new FluidSimulationConstantsConfigInitializeTag());
		}
	}
}