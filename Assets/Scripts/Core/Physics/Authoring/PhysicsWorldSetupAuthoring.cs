using AlchemicalArts.Core.Naming.Authoring;
using AlchemicalArts.Core.Physics.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Authoring
{
	[RequireComponent(typeof(EntityHierarchyAuthoring))]
	public class PhysicsWorldSetupAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public PhysicsWorldDefinition WorldDefinition { get; private set; } = PhysicsWorldDefinition.defaultDefinition;
	}

	public class PhysicsWorldAuthoringBaker : Baker<PhysicsWorldSetupAuthoring>
	{
		public override void Bake(PhysicsWorldSetupAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new PhysicsWorldSetup() { worldDefinition = authoring.WorldDefinition });
			AddComponent(entity, new PhysicsWorldState());
		}
	}
}