using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using PotionCraft.Core.Fluid.Simulation.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(GravitySystem))]
	partial struct LiquidInwardsInputSystem : ISystem
	{
		public JobHandle handle;


		private NativeList<int> output;


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
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<LiquidPositionInitializationSystem>();
			ref var applyGravitySystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<GravitySystem>();
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
						var collectAffectedParticlesJob = new CollectAffectedParticlesJob
						{
							output = output.AsParallelWriter(),
							positions = populateLiquidPositionsSystem.positionBuffer,
							fluidInputConfig = fluidInputConfig.ValueRO,
						};
						handle = collectAffectedParticlesJob.ScheduleParallel(handle);
						break;
					}
					
					var applyInputToCache = new ApplyInwardsForcesJob
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
	}
}