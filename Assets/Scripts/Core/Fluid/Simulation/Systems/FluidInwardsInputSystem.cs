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
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(GravitySystem))]
	partial struct FluidInwardsInputSystem : ISystem
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

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var gravitySystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<GravitySystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;
			
			var draggingModeEntity = SystemAPI.GetSingletonEntity<DraggingParticlesModeState>();
			var draggingParticlesModeState = SystemAPI.GetComponentRO<DraggingParticlesModeState>(draggingModeEntity);
			var fluidInputState = SystemAPI.GetComponentRO<FluidInputState>(draggingModeEntity);
			var fluidInputConfig = SystemAPI.GetComponentRW<FluidInputConfig>(draggingModeEntity);

			handle = gravitySystem.handle;
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
							positions = fluidPositionInitializationSystem.positionBuffer,
							fluidInputConfig = fluidInputConfig.ValueRO,
						};
						handle = collectAffectedParticlesJob.ScheduleParallel(handle);
						break;
					}
					
					var applyInputToCache = new ApplyInwardsForcesJob
					{
						velocities = fluidPositionInitializationSystem.velocityBuffer,
						positions = fluidPositionInitializationSystem.positionBuffer,
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