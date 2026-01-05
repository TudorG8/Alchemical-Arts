using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup), OrderFirst = true)]
	public partial struct SimulationBuffersSystem : ISystem
	{
		public int count;
		
		public NativeArray<float2> positionBuffer;
		
		public NativeArray<float2> velocityBuffer;

		public NativeArray<float2> predictedPositionsBuffer;

		public NativeArray<SpatialEntry> spatialBuffer;
		
		public NativeArray<int> spatialOffsetsBuffer;

		public NativeArray<float> densityBuffer;

		public NativeArray<float> nearDensityBuffer;

		public NativeList<int> inwardsForceBuffer;

		public NativeArray<BatchVelocity> batchVelocityBuffer;

		public int hashingLimit;


		private EntityQuery fluidQuery;

		private int bufferCapacity;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			bufferCapacity = 10000;
			positionBuffer = new NativeArray<float2>(bufferCapacity, Allocator.Persistent);
			velocityBuffer = new NativeArray<float2>(bufferCapacity, Allocator.Persistent);
			predictedPositionsBuffer = new NativeArray<float2>(bufferCapacity, Allocator.Persistent);
			spatialBuffer = new NativeArray<SpatialEntry>(bufferCapacity, Allocator.Persistent);
			spatialOffsetsBuffer = new NativeArray<int>(bufferCapacity, Allocator.Persistent);
			densityBuffer = new NativeArray<float>(bufferCapacity, Allocator.Persistent);
			nearDensityBuffer = new NativeArray<float>(bufferCapacity, Allocator.Persistent);
			inwardsForceBuffer = new NativeList<int>(bufferCapacity, Allocator.Persistent);
			batchVelocityBuffer = new NativeArray<BatchVelocity>(bufferCapacity, Allocator.Persistent);
			fluidQuery = SystemAPI.QueryBuilder()
				.WithAll<SimulatedItemTag>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			positionBuffer.Dispose();
			velocityBuffer.Dispose();
			predictedPositionsBuffer.Dispose();
			spatialBuffer.Dispose();
			spatialOffsetsBuffer.Dispose();
			densityBuffer.Dispose();
			nearDensityBuffer.Dispose();
			inwardsForceBuffer.Dispose();
			batchVelocityBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			count = fluidQuery.CalculateEntityCount();
			hashingLimit = math.min(count, bufferCapacity);
		}
	}
}