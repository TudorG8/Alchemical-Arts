using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class floatExtensions
	{
		public static Quaternion ToQuaternion(this float input)
		{
			return Quaternion.Euler(0f, 0f, input * Mathf.Rad2Deg);
		}
	}
}