using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Models;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[assembly: RegisterGenericJobType(typeof(WritePartionedIndexJob<SpatiallyPartionedIndex>))]

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup), OrderFirst = true)]
	public partial struct SpatialCoordinatorSystem : ISystem
	{
		public JobHandle handle;

		public int count;
		
		public NativeArray<SpatialEntry> spatialBuffer;

		public NativeArray<int> spatialOffsetsBuffer;
		
		public NativeArray<float2> positionBuffer;
		
		public NativeArray<float2> velocityBuffer;

		public NativeArray<float2> predictedPositionsBuffer;

		public EntityQuery simulatedQuery;

		public int hashingLimit;


		private int bufferCapacity;

		private ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexTypeHandle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			
			bufferCapacity = 10000;
			hashingLimit = 10000;
			positionBuffer = new NativeArray<float2>(bufferCapacity, Allocator.Persistent);
			velocityBuffer = new NativeArray<float2>(bufferCapacity, Allocator.Persistent);
			predictedPositionsBuffer = new NativeArray<float2>(bufferCapacity, Allocator.Persistent);
			spatialBuffer = new NativeArray<SpatialEntry>(bufferCapacity, Allocator.Persistent);
			spatialOffsetsBuffer = new NativeArray<int>(bufferCapacity, Allocator.Persistent);
			
			simulatedQuery = SystemAPI.QueryBuilder()
				.WithAll<SpatiallyPartionedIndex>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
			spatialIndexTypeHandle = state.GetComponentTypeHandle<SpatiallyPartionedIndex>(isReadOnly: false);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			positionBuffer.Dispose();
			velocityBuffer.Dispose();
			predictedPositionsBuffer.Dispose();
			spatialBuffer.Dispose();
			spatialOffsetsBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			count = simulatedQuery.CalculateEntityCount();
			if (count == 0)
				return;

			spatialIndexTypeHandle.Update(ref state);
			handle = PartitionedIndexJobUtility.ScheduleWritePartitionedIndex(simulatedQuery, spatialIndexTypeHandle, state.Dependency);
		}
	}
}