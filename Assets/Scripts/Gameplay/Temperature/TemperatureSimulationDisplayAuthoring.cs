using AlchemicalArts.Shared.Models;
using AlchemicalArts.Shared.Utility;
using Unity.Entities;
using UnityEngine;

public class TemperatureSimulationDisplayAuthoring : MonoBehaviour
{
	[field: SerializeField]
	public Shader Shader { get; private set; }
	
	[field: SerializeField]
	public Mesh Mesh { get; private set;}
	
	[field: SerializeField]
	public Gradient Gradient { get; private set;}
	
	[field: SerializeField]
	public MinMaxFloatValue TemperatureBounds { get; private set;}
	
	[field: SerializeField]
	public int GradientResolution { get; private set;}
	
	[field: SerializeField]
	public float ParticleSize { get; private set;}
}

public class TemperatureSimulationDisplayAuthoringBaker : Baker<TemperatureSimulationDisplayAuthoring>
{
	public override void Bake(TemperatureSimulationDisplayAuthoring authoring)
	{
		var entity = GetEntity(TransformUsageFlags.None);
		AddComponentObject(entity, new TemperatureSimulationDisplayConfig() 
		{
			shader = authoring.Shader,
			mesh = authoring.Mesh,
			temperatureBounds = authoring.TemperatureBounds,
			particleSize = authoring.ParticleSize,
			texture = TextureUtility.TextureFromGradient(authoring.GradientResolution, authoring.Gradient)
		});
	}
}
