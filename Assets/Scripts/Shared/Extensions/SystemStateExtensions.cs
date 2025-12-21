using Unity.Entities;

public static class SystemStateExtensions
{
	public static ref T GetUnmanagedSystemRefWithoutHandle<T>(this WorldUnmanaged worldUnmanaged) where T : unmanaged, ISystem
	{
		var handle = worldUnmanaged.GetExistingUnmanagedSystem<T>();

		return ref worldUnmanaged.GetUnsafeSystemRef<T>(handle);
	}
}