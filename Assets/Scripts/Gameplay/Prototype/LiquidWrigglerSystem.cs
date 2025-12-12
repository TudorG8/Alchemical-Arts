using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Systems
{
	public partial struct LiquidWrigglerSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach(var (localTransform, physicsVelocity, wriggler, wrigglerTargetBuffer) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRW<_WrigglerData>, DynamicBuffer<_WrigglerTargetBufferData>>())
			{
				var angularVelocity = new float3(0, 0, SystemAPI.Time.DeltaTime * wriggler.ValueRO.RotationSpeed);
				physicsVelocity.ValueRW.Angular = angularVelocity;

				var currentTarget = wrigglerTargetBuffer[wriggler.ValueRO.WaypointIndex];
				var direction = currentTarget.WaypointLocation - localTransform.ValueRW.Position;
				if (math.length(direction) < 0.1f)
				{
					wriggler.ValueRW.WaypointIndex = (wriggler.ValueRO.WaypointIndex + 1) % wrigglerTargetBuffer.Length;
				}
				physicsVelocity.ValueRW.Linear = SystemAPI.Time.DeltaTime * wriggler.ValueRO.MovementSpeed * direction;
			}
		}
	}
}