using PotionCraft.Core.Naming.Authoring;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Shared.Extensions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Authoring
{
	public class PhysicsBodyAuthoring : MonoBehaviour, IDrawableShape
	{
		[field: SerializeField]
		public PhysicsBodyDefinition BodyDefinition { get; set; } = PhysicsBodyDefinition.defaultDefinition;


		public PhysicsBodyDefinition ToBody()
		{
			var bodyDefinition = BodyDefinition;
			bodyDefinition.position = transform.position;
			bodyDefinition.rotation = transform.ToPhysicsRotate();

			return bodyDefinition;
		}
	}
	
	public class PhysicsBodyAuthoringBaker : Baker<PhysicsBodyAuthoring>
	{
		public override void Bake(PhysicsBodyAuthoring authoring)
		{
			var bodyDefinition = authoring.ToBody();

			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new PhysicsBodySetupComponent() { bodyDefinition = bodyDefinition });
		}
	}
}