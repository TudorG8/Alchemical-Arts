using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Display.Groups
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public partial class FluidRenderingGroup : ComponentSystemGroup { }
}
