using Unity.Entities;
using UnityEngine;

public class TemperatureAuthoring : MonoBehaviour
{
	[field: SerializeField]
	public float Temperature { get; private set; }
}

public class TemperatureAuthoringBaker : Baker<TemperatureAuthoring>
{
	public override void Bake(TemperatureAuthoring authoring)
	{
		var entity = GetEntity(TransformUsageFlags.Dynamic);
		AddComponent(entity, new TemperaturePartionedIndex());
		AddComponent(entity, new TemperatureState() { temperature = authoring.Temperature});
	}
}
