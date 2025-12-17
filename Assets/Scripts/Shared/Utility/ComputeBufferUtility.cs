using UnityEngine;

namespace PotionCraft.Shared.Utility
{
	public static class ComputeBufferUtility
	{
		private	const int UINT_STRIDE = sizeof(uint);
		
		private	const int ARG_COUNT = 5;
		
		private	const int SUBMESH_INDEX = 0;

		private static readonly uint[] argsBufferArray = new uint[5];
		
		
		public static void CreateArgsBuffer(ref ComputeBuffer argsBuffer, Mesh mesh, uint numInstances)
		{
			if (argsBuffer == null || !argsBuffer.IsValid() || argsBuffer.count != argsBufferArray.Length)
			{
				argsBuffer?.Release();
				argsBuffer = new ComputeBuffer(ARG_COUNT, UINT_STRIDE, ComputeBufferType.IndirectArguments);
			}

			lock (argsBufferArray)
			{
				argsBufferArray[0] = mesh.GetIndexCount(SUBMESH_INDEX);
				argsBufferArray[1] = numInstances;
				argsBufferArray[2] = mesh.GetIndexStart(SUBMESH_INDEX);
				argsBufferArray[3] = mesh.GetBaseVertex(SUBMESH_INDEX);
				
				argsBuffer.SetData(argsBufferArray);
			}
		}
	}
}