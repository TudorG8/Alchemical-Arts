using PotionCraft.Core.Naming.Authoring;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Shared.Extensions;
using PotionCraft.Shared.Scopes;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public interface IDrawableShape
{

}

namespace PotionCraft.Core.Physics.Authoring
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(CircleCollider2D))]
	[RequireComponent(typeof(EntityHierarchyAuthoring))]
	public class PhysicsCircleSetupAuthoring : MonoBehaviour, IDrawableShape
	{
		[field: SerializeField]
		public PhysicsBodyAuthoring PhysicsBodyAuthoring { get; set; }

		[field: SerializeField]
		public CircleCollider2D CircleCollider2D { get; set; }

		[field: SerializeField]
		public PhysicsShapeDefinition ShapeDefinition { get; set; } = PhysicsShapeDefinition.defaultDefinition;


		public CircleGeometry ToCircleGeometry()
		{
			var body = PhysicsBodyAuthoring.transform;
			var shape = transform;

			var radius = CircleCollider2D.radius * shape.ToCircleScale();
			var position = shape.position - body.position;
			var circleGeometry = new CircleGeometry() { center = position, radius = radius };

			return circleGeometry;
		}


		private void OnValidate()
		{
			PhysicsBodyAuthoring = this.ValidateComponent(PhysicsBodyAuthoring);
			CircleCollider2D = this.ValidateComponent(CircleCollider2D);
		}
	}

	public class PhysicsCircleSetupAuthoringBaker : Baker<PhysicsCircleSetupAuthoring>
	{
		public override void Bake(PhysicsCircleSetupAuthoring authoring)
		{
			DependsOn(authoring.PhysicsBodyAuthoring);
			DependsOn(authoring.CircleCollider2D);

			var entity = GetEntity(TransformUsageFlags.Dynamic);
			var physicsBody = GetEntity(authoring.PhysicsBodyAuthoring, TransformUsageFlags.Dynamic);
			var circleGeometry = authoring.ToCircleGeometry();

			AddComponent(entity, new PhysicsCircleSetupComponent()
			{
				shapeDefinition = authoring.ShapeDefinition,
				circleGeometry = circleGeometry,
				bodyEntity = physicsBody
			});
		}
	}
}