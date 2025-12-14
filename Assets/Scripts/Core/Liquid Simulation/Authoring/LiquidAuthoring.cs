using PotionCraft.Core.LiquidSimulation.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.LiquidSimulation.Authoring
{
	public class LiquidAuthoring : MonoBehaviour
	{
		public class LiquidAuthoringBaker : Baker<LiquidAuthoring>
		{
			public override void Bake(LiquidAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new LiquidTag());
			}
		}
	}
}