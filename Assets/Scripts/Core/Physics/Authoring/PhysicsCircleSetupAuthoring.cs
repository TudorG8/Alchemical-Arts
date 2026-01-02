using AlchemicalArts.Core.Naming.Authoring;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.Physics.Extensions;
using AlchemicalArts.Shared.Extensions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Authoring
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(CircleCollider2D))]
	[RequireComponent(typeof(EntityHierarchyAuthoring))]
	public class PhysicsCircleSetupAuthoring : MonoBehaviour, IDrawableShape
	{
		[field: SerializeField]
		public PhysicsBodyAuthoring PhysicsBodyAuthoring { get; private set; }

		[field: SerializeField]
		public CircleCollider2D CircleCollider2D { get; private set; }

		[field: SerializeField]
		public PhysicsShapeDefinition ShapeDefinition { get; private set; } = PhysicsShapeDefinition.defaultDefinition;


		public CircleGeometry ToCircleGeometry(PhysicsTransform offset)
		{
			var resultMatrix = offset.MultiplyBy(-1).ToMatrix() * transform.localToWorldMatrix;

			var center = resultMatrix.MultiplyPoint3x4(CircleCollider2D.offset);
			var radius = CircleCollider2D.radius * transform.ToCircleScale();

			var circleGeometry = new CircleGeometry() { center = center, radius = radius };

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
			var circleGeometry = authoring.ToCircleGeometry(authoring.PhysicsBodyAuthoring.transform.ToPhysicsTransform());

			AddComponent(entity, new PhysicsCircleSetup()
			{
				shapeDefinition = authoring.ShapeDefinition,
				circleGeometry = circleGeometry,
				bodyEntity = physicsBody
			});
			AddComponent(entity, new PhysicsShapeState());
			SetComponentEnabled<PhysicsShapeState>(entity, false);
		}
	}
}