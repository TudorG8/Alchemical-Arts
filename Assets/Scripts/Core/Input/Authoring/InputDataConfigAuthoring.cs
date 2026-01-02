using AlchemicalArts.Core.Input.Components;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Input.Authoring
{
	public class InputDataAuthoring : MonoBehaviour
	{
		
	}

	public class InputDataAuthoringBaker : Baker<InputDataAuthoring>
	{
		public override void Bake(InputDataAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent<InputDataConfig>(entity);
		}
	}
}