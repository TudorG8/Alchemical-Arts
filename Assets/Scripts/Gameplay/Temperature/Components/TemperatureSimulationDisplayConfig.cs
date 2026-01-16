using AlchemicalArts.Shared.Models;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Temperature.Components
{
	public class TemperatureSimulationDisplayConfig : IComponentData
	{
		public Shader shader;

		public Mesh mesh;

		public Texture2D texture;
		
		public MinMaxFloatValue temperatureBounds;
		
		public float particleSize;
	}
}