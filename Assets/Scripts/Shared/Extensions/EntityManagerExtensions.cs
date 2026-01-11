using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class EntityManagerExtensions
	{
		public static async Awaitable<T> GetComponentDataAsync<T>(this EntityManager entityManager) where T : unmanaged, IComponentData
		{
			var query = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);
			await query.WaitForEntityToExist();
			return query.GetSingleton<T>();
		}
	}
}