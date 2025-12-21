using System.Collections.Generic;
using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Collections.NativeSortExtension;


namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	public struct SpatialEntryKeyComparer : IComparer<SpatialEntry>
	{
		public readonly int Compare(SpatialEntry a, SpatialEntry b)
			=> a.key.CompareTo(b.key);
	}

	public struct SpatialEntry
	{
		public int index;
		
		public int key;
	}

	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(CalculatePredictedPositionsSystem))]
	partial struct SpatialDataSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<SpatialEntry> Spatial;
		
		public NativeArray<int> SpatialOffsets;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();
			Spatial = new NativeArray<SpatialEntry>(10000, Allocator.Persistent);
			SpatialOffsets = new NativeArray<int>(10000, Allocator.Persistent);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			Spatial.Dispose();
			SpatialOffsets.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PopulateLiquidPositionsSystem>();
			ref var calculatePredictedPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<CalculatePredictedPositionsSystem>();

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var populateSpatialOutputJob = new PopulateSpatialOutputJob
			{
				predictedPositions = calculatePredictedPositionsSystem.predictedPositionsBuffer,
				radius = simulationConfig.radius,
				count = populateLiquidPositionsSystem.count,
				spatialOutput = Spatial,
				spatialOffsetOutput = SpatialOffsets

			};
			state.Dependency = populateSpatialOutputJob.ScheduleParallel(calculatePredictedPositionsSystem.handle);

			var sortSpatialJob = new SortSpatialJob
			{
				Spatial = Spatial,
				count = populateLiquidPositionsSystem.count
			};
			state.Dependency = sortSpatialJob.Schedule(state.Dependency);

			var reorderIndicesJob = new ReorderIndicesJob()
			{
				spatial = Spatial,
				spatialOffsets = SpatialOffsets
			};
			handle = state.Dependency = reorderIndicesJob.ScheduleParallel(state.Dependency);
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		[WithAll(typeof(PhysicsBodyState))]
		public partial struct PopulateSpatialOutputJob : IJobEntity
		{
			[ReadOnly]
			public NativeArray<float2> predictedPositions;
			
			[ReadOnly]
			public float radius;

			[ReadOnly]
			public int count;

			[WriteOnly]

			public NativeSlice<SpatialEntry> spatialOutput;

			[WriteOnly]
			public NativeArray<int> spatialOffsetOutput;


			void Execute(
				[EntityIndexInQuery] int index)
			{
				var cell = GetCell2D(predictedPositions[index], radius);
				var hash = HashCell2D(cell);
				var cellKey = KeyFromHash(hash, 10000);
				
				spatialOutput[index] = new SpatialEntry() { index = index, key = (int)cellKey };
				spatialOffsetOutput[index] = int.MaxValue;
			}

			public uint HashCell2D(float2 input)
			{
				var a = (uint)input.x * 15823;
				var b = (uint)input.y * 9737333;
				return a + b;
			}

			public int2 GetCell2D(float2 input, float radius)
				{
					var x = (int)math.floor(input.x / radius);
					var y = (int)math.floor(input.y / radius);

					return new int2(x, y);
				}

			public readonly uint KeyFromHash(uint hash, int length)
			{
				return hash % (uint)length;
			}
		}

		[BurstCompile]
		public struct SortSpatialJob : IJob
		{
			public NativeArray<SpatialEntry> Spatial;

			public int count;

			public void Execute()
			{
				var slice = new NativeSlice<SpatialEntry>(Spatial, 0, count);
				var comparer = new SpatialEntryKeyComparer();
				slice.Sort(comparer);
			}
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		[WithAll(typeof(PhysicsBodyState))]
		public partial struct ReorderIndicesJob : IJobEntity
		{
			[NativeDisableParallelForRestriction]
			public NativeArray<SpatialEntry> spatial;

			[NativeDisableParallelForRestriction]
			public NativeArray<int> spatialOffsets;
			
			void Execute(
				[EntityIndexInQuery] int index)
			{
				var key = spatial[index].key;
				var prevKey = index == 0 ? int.MaxValue : spatial[index - 1].key;
				if (key != prevKey)
				{
					spatialOffsets[key] = index;
				}
			}
		}
	}
}