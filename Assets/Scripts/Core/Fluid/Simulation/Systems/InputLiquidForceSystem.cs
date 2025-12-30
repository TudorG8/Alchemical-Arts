using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(ApplyGravitySystem))]
	partial struct InputLiquidForceSystem : ISystem
	{
		public JobHandle handle;

		NativeList<int> output;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			output = new NativeList<int>(10000, Allocator.Persistent);
			state.RequireForUpdate<InputDataState>();
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			output.Dispose();
		}

		public void OnUpdate(ref SystemState state)
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PopulateLiquidPositionsSystem>();
			ref var applyGravitySystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<ApplyGravitySystem>();
			if (populateLiquidPositionsSystem.count == 0)
				return;
			
			var draggingModeEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRO<DraggingParticlesModeState>(draggingModeEntity);
			var fluidInputState = SystemAPI.GetComponentRO<FluidInputState>(draggingModeEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingModeEntity);

			handle = applyGravitySystem.handle;
			switch(draggingParticlesModeState.ValueRO.mode)
			{
				case DraggingParticlesMode.Idle:
					output.Clear();
					break;
				case DraggingParticlesMode.Inwards:
					if (output.Length == 0)
					{
						var collectMatchingIndicesJob = new CollectFluidInAreaJob
						{
							output = output.AsParallelWriter(),
							positions = populateLiquidPositionsSystem.positionBuffer,
							fluidInputConfig = fluidInputConfig.ValueRO,
						};
						handle = collectMatchingIndicesJob.ScheduleParallel(handle);
						break;
					}
					
					var applyInputToCache = new ApplyInputToCache
					{
						velocities = populateLiquidPositionsSystem.velocityBuffer,
						positions = populateLiquidPositionsSystem.positionBuffer,
						fluidInputConfig = fluidInputConfig.ValueRO,
						fluidInputState = fluidInputState.ValueRO,
						deltaTime = SystemAPI.Time.DeltaTime,
						indexes = output,
					};
					handle = applyInputToCache.Schedule(output.Length, 64, handle);
					break;
			}
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		public partial struct CollectFluidInAreaJob : IJobEntity
		{
			[ReadOnly]
			public NativeArray<float2> positions;

			[ReadOnly]
			public FluidInputConfig fluidInputConfig;
			
			[WriteOnly]
			public NativeList<int>.ParallelWriter output;


			public void Execute([EntityIndexInQuery] int index)
			{
				var offset = fluidInputConfig.position - positions[index];
				var sqrDst = math.dot(offset, offset);
				if (sqrDst < fluidInputConfig.interactionRadius * fluidInputConfig.interactionRadius)
				{
					output.AddNoResize(index);
				}
			}
		}

		[BurstCompile]
		public partial struct ApplyInputToCache : IJobParallelFor
		{
			[NativeDisableParallelForRestriction]
			public NativeArray<float2> velocities;

			[ReadOnly]
			public NativeArray<float2> positions;

			[ReadOnly]
			public NativeList<int> indexes;

			[ReadOnly]
			public FluidInputConfig fluidInputConfig;

			[ReadOnly]
			public FluidInputState fluidInputState;

			[ReadOnly]
			public float deltaTime;


			public void Execute(int index)
			{
				var actualIndex = indexes[index];

				var offset = fluidInputConfig.position - positions[actualIndex];
				var distance = math.length(offset);
				var direction = distance <= float.Epsilon ? float2.zero : offset / distance;
				var smoothingToCenter = math.clamp(distance / fluidInputConfig.interactionRadius, 0, 1);

				var springForce = smoothingToCenter * fluidInputState.interactionStrength * direction;
				var dampingForce = smoothingToCenter * fluidInputState.damping * -velocities[actualIndex];

				velocities[actualIndex] += (springForce + dampingForce) * deltaTime;
			}
		}
	}
}