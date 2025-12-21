using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(PopulateLiquidPositionsSystem))]
	partial struct InputLiquidForceSystem : ISystem
	{
		public JobHandle handle;

		private SystemHandle populateLiquidPositionsSystemHandle;

		private SystemHandle grav;

		private float interactionStrength;

		private float interactionRadius;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			interactionRadius = 3f;
			interactionStrength = 200f;
			populateLiquidPositionsSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();
			grav = state.WorldUnmanaged.GetExistingUnmanagedSystem<ApplyGravitySystem>();
			state.RequireForUpdate<InputDataState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);
			ref var grav2 = ref state.WorldUnmanaged.GetUnsafeSystemRef<ApplyGravitySystem>(grav);
			if (populateLiquidPositionsSystem.count == 0)
				return;
			
			var inputData = SystemAPI.GetSingleton<InputDataState>();
			
			var isPullInteraction = inputData.primaryPressed;
			var isPushInteraction = inputData.secondaryPressed;
			var currInteractStrength = 0f;
			if (isPushInteraction || isPullInteraction)
			{
				currInteractStrength = isPushInteraction ? -interactionStrength : interactionStrength;
			}

			var applyGravityJob = new ApplyUserInputJob
			{
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				positions = populateLiquidPositionsSystem.positionBuffer,
				input = inputData.worldPosition,
				radius = interactionRadius,
				strength = currInteractStrength,
				deltaTime = SystemAPI.Time.DeltaTime,
			};
			handle = applyGravityJob.ScheduleParallel(grav2.handle);
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		[WithAll(typeof(PhysicsBodyState))]
		public partial struct ApplyUserInputJob : IJobEntity
		{
			public NativeArray<float2> velocities;

			[ReadOnly]
			public NativeArray<float2> positions;

			[ReadOnly]
			public float2 input;

			[ReadOnly]
			public float radius;
			
			[ReadOnly]
			public float strength;

			[ReadOnly]
			public float deltaTime;


			void Execute(
				[EntityIndexInQuery] int index)
			{
				var interactionForce = float2.zero;
				var offset = input - positions[index];
				var sqrDst = math.dot(offset, offset);
				if (sqrDst < radius * radius)
				{
					var dst = math.sqrt(sqrDst);
					var dir = dst <= float.Epsilon ? float2.zero : offset / dst;
					var centreT = 1 - dst/radius;
					interactionForce += (dir * strength - velocities[index]) * centreT;
				}

				velocities[index] += interactionForce * deltaTime;
			}
		}
	}
}