using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class MonoBehaviourExtensions
	{
		public static T ValidateComponent<T>(this MonoBehaviour input, T field) where T : Behaviour
		{
			if (field == null)
			{
				var component = input.GetComponentInParent<T>();
				if (component != null)
				{
					return component;
				}
			}

			return field;
		}
	}
}