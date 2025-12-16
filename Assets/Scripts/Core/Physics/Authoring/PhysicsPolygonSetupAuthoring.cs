using System.Linq;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Extensions;
using PotionCraft.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Authoring
{
	[RequireComponent(typeof(PolygonCollider2D))]
	public class PhysicsPolygonSetupAuthoring : MonoBehaviour, IDrawableShape
	{
		[field: SerializeField]
		public PhysicsBodyAuthoring PhysicsBodyAuthoring { get; set; }

		[field: SerializeField]
		public PolygonCollider2D PolygonCollider2D { get; set; }

		[field: SerializeField]
		public PhysicsShapeDefinition ShapeDefinition { get; set; } = PhysicsShapeDefinition.defaultDefinition;


		public NativeArray<PolygonGeometry> ToGeometry(PhysicsTransform offset)
		{
			var resultMatrix = offset.MultiplyBy(-1).ToMatrix() * transform.localToWorldMatrix;

			var span = PolygonCollider2D.points.Select(p => resultMatrix.MultiplyPoint3x4(p + PolygonCollider2D.offset).To2D()).ToArray();

			var composer = PhysicsComposer.Create();
			composer.AddLayer(span, PhysicsTransform.identity);
			var polygons = composer.CreatePolygonGeometry(Vector2.one, Allocator.Temp);
			composer.Destroy();

			return polygons;
		}


		private void OnValidate()
		{
			PhysicsBodyAuthoring = this.ValidateComponent(PhysicsBodyAuthoring);
			PolygonCollider2D = this.ValidateComponent(PolygonCollider2D);
		}
	}
	
	public class PhysicsPolygonSetupAuthoringBaker : Baker<PhysicsPolygonSetupAuthoring>
	{
		public override void Bake(PhysicsPolygonSetupAuthoring authoring)
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
			foreach(var g in geometry)
			{
				buffer.Add(new PolygonGeometryData() { geometry = g });
			}
		}
	}
}