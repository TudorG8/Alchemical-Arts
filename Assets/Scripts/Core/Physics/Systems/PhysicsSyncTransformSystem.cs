using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	partial struct PhysicsSyncTransformSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsBodyConfigComponent>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach (var (bodyConfig, localTransform) in SystemAPI.Query<PhysicsBodyConfigComponent, RefRW<LocalTransform>>())
			{
				var position = bodyConfig.physicsBody.position;
				var rotation = bodyConfig.physicsBody.rotation.ToECSQuaternion();
				// var angle = math.atan2(rotation.y, rotation.x);
				localTransform.ValueRW.Position = new float3(position.x, position.y, 0);
				localTransform.ValueRW.Rotation = rotation;
			}
		}
	}
}