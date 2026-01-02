using PotionCraft.Gameplay.Prototype.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Prototype.Jobs
{
	[BurstCompile]
	public partial struct SpawnFluidEntitiesJob : IJobEntity
	{
		[WriteOnly]
		public EntityCommandBuffer.ParallelWriter ecb;

		[ReadOnly]
		public double elapsedTime;

		[ReadOnly]
		public uint baseSeed;

		[ReadOnly]
		public Entity folder;


		void Execute(
			[EntityIndexInQuery] int index,
			ref FluidSpawnerState fluidSpawnerState,
			in FluidSpawnerConfig fluidSpawnerConfig,
			in LocalToWorld localToWorld)
		{
			if (fluidSpawnerState.timer == 0) 
			{
				fluidSpawnerState.timer = elapsedTime + fluidSpawnerConfig.delay; return;
			}
			
			if (fluidSpawnerState.timer > elapsedTime) return;


			if (fluidSpawnerState.count >= fluidSpawnerConfig.max) return; 

			var random = new Random(baseSeed + (uint)index);

			var obj = ecb.Instantiate(index, fluidSpawnerConfig.fluid);
			ecb.SetComponent(index, obj, LocalTransform.FromPosition(localToWorld.Position + new float3(random.NextFloat(-0.1f, 0.1f), random.NextFloat(-0.1f, 0.1f), 0)));
			ecb.AddComponent(index, obj, new Parent() { Value = folder });
			ecb.AddComponent(index, obj, new PreviousParent() { Value = folder });
			
			fluidSpawnerState.count++;
			fluidSpawnerState.timer = elapsedTime + fluidSpawnerConfig.delay;
		}
	}
}