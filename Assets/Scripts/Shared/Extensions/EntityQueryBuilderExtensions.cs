using Unity.Entities;

namespace AlchemicalArts.Shared.Extensions
{
	public static class EntityQueryBuilderExtensions
	{
		public static EntityQuery BuildAndReset(this EntityQueryBuilder input, EntityManager entityManager)
		{
			var query = input.Build(entityManager);
			input.Reset();
			return query;
		}
	}
}