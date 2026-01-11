using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class EntityExtensions
	{
		public static async Awaitable WaitForEntityToExist(this EntityQuery entityQuery)
		{
			while (entityQuery.CalculateEntityCount() == 0)
			{
				await Awaitable.EndOfFrameAsync();
			}
		}
	}
}