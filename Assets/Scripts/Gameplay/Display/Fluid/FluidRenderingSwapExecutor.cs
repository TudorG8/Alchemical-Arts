using AlchemicalArts.Core.Fluid.Display.Groups;
using AlchemicalArts.Core.Fluid.Display.Systems;
using AlchemicalArts.Gameplay.Display.Fluid.Components;
using AlchemicalArts.Gameplay.Temperature.Systems;
using Unity.Burst;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Display.Fluid.Systems
{
	[UpdateInGroup(typeof(FluidRenderingGroup), OrderFirst = true)]
	public partial struct FluidRenderingSwapExecutor : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<FluidRenderingState>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var fluidRenderingSystemBase = state.World.GetOrCreateSystemManaged<FluidRenderingSystemBase>();
			var temperatureRenderingSystemBase = state.World.GetOrCreateSystemManaged<TemperatureRenderingSystemBase>();

			var fluidRenderingState = SystemAPI.GetSingleton<FluidRenderingState>();

			fluidRenderingSystemBase.Enabled = fluidRenderingState.mode == Models.FluidRenderingMode.Default;
			temperatureRenderingSystemBase.Enabled = fluidRenderingState.mode == Models.FluidRenderingMode.Temperature;
		}
	}
}