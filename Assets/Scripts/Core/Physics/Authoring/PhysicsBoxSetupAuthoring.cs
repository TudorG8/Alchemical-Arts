using System.Linq;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.Physics.Extensions;
using AlchemicalArts.Shared.Extensions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Authoring
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class PhysicsBoxSetupAuthoring : MonoBehaviour, IDrawableShape
	{
		[field: SerializeField]
		public PhysicsBodyAuthoring PhysicsBodyAuthoring { get; private set; }

		[field: SerializeField]
		public BoxCollider2D BoxCollider2D { get; private set; }

		[field: SerializeField]
		public PhysicsShapeDefinition ShapeDefinition { get; private set; } = PhysicsShapeDefinition.defaultDefinition;


		public PolygonGeometry ToGeometry(PhysicsTransform offset)
		{
			var resultMatrix = offset.MultiplyBy(-1).ToMatrix() * transform.localToWorldMatrix;

			var localVerts = BoxCollider2D.ToLocalCorners().ToArray();

			return PolygonGeometry.Create(localVerts, BoxCollider2D.edgeRadius, resultMatrix);
		}


		private void OnValidate()
		{
			PhysicsBodyAuthoring = this.ValidateComponent(PhysicsBodyAuthoring);
			BoxCollider2D = this.ValidateComponent(BoxCollider2D);
		}
	}
	
	public class PhysicsBoxSetupAuthoringBaker : Baker<PhysicsBoxSetupAuthoring>
	{
		public override void Bake(PhysicsBoxSetupAuthoring authoring)
		{
			DependsOn(authoring.PhysicsBodyAuthoring);

			var physicsBody = GetEntity(authoring.PhysicsBodyAuthoring, TransformUsageFlags.Dynamic);
			var geometry = authoring.ToGeometry(authoring.PhysicsBodyAuthoring.transform.ToPhysicsTransform());

			var entity = GetEntity(TransformUsageFlags.Dynamic);
			var buffer = AddBuffer<PolygonGeometryData>(entity);
			AddComponent(entity, new PhysicsBoxSetup()
			{
				shapeDefinition = authoring.ShapeDefinition,
				bodyEntity = physicsBody
			});
			buffer.Add(new PolygonGeometryData() { geometry = geometry });
		}
	}
}