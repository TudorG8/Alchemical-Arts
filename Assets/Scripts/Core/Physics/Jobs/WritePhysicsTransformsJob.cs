

using PotionCraft.Core.Physics.Extensions;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using static UnityEngine.LowLevelPhysics2D.PhysicsEvents;

namespace PotionCraft.Core.Physics.Jobs
{
	[BurstCompile]
	public partial struct WritePhysicsTransformsJob : IJobParallelFor
	{
		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<BodyUpdateEvent> bodyUpdateEvents;
		
		[NativeDisableParallelForRestriction]
		public ComponentLookup<LocalTransform> localTransformLookup;

		public void Execute(int index)
		{
			var bodyUpdate = bodyUpdateEvents[index];
			var entity = bodyUpdate.body.userData.physicsMaskValue.DecodeFromPhysicsMask();
			var transform = localTransformLookup[entity];

			var position = bodyUpdate.transform.position.ToFloat3();
			var rotation = bodyUpdate.transform.rotation.ToECSQuaternion();
			transform.Position = position;
			transform.Rotation = rotation;

			localTransformLookup[entity] = transform;
		}
	}
}