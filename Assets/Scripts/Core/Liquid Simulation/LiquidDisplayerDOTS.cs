using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Gameplay.Authoring;
using PotionCraft.Shared.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LiquidDisplayerDOTS : MonoBehaviour
{
	public Shader shader;

	public Mesh mesh;

	public Gradient gradient;

	public float maxVelocity;

	public int gradientResolution;
	
	public float particleSize;

	Material material;

	Bounds bounds;

	ComputeBuffer argsBuffer;

	ComputeBuffer positionsBuffer;
	
	ComputeBuffer velocitiesBuffer;

	Texture2D gradientTexture;


	void Start()
	{
		material = new Material(shader);
		bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

		TextureFromGradient(ref gradientTexture, gradientResolution, gradient);
		positionsBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
		velocitiesBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());

		material.SetBuffer("Positions", positionsBuffer);
		material.SetBuffer("Velocities", velocitiesBuffer);
		material.SetFloat("Radius", particleSize);
		material.SetFloat("MaxVelocity", maxVelocity);
		material.SetTexture("ColourMap", gradientTexture);
	}


	void LateUpdate()
	{
		UpdateSettings();
	}

	private void UpdateSettings()
	{
		if (argsBuffer != null)
		{
			argsBuffer.Release();
		}
		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var worldQuerry = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldConfigComponent>().Build(entityManager);
		var liquidQuerry = new EntityQueryBuilder(Allocator.Temp).WithAll<_LiquidTag>().WithAll<LocalTransform>().WithAll<PhysicsBodyConfigComponent>().Build(entityManager);
		var numParticles = liquidQuerry.CalculateEntityCount();

		if (worldQuerry.CalculateEntityCount() == 0)
			return;


		var populateLiquidPositionsSystemHandle = World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();
		if (!World.DefaultGameObjectInjectionWorld.Unmanaged.IsSystemValid(populateLiquidPositionsSystemHandle))
		{
			return;
		}

		ref PopulateLiquidPositionsSystem system = ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);

		positionsBuffer.SetData(system.positionBuffer, 0, 0, numParticles);
		velocitiesBuffer.SetData(system.velocityBuffer, 0, 0, numParticles);
		CreateArgsBuffer(ref argsBuffer, mesh, numParticles);
		Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
		
	}

	private void OnDestroy()
	{
		positionsBuffer?.Dispose();
		velocitiesBuffer?.Dispose();
	}

	public static void TextureFromGradient(ref Texture2D texture, int width, Gradient gradient, FilterMode filterMode = FilterMode.Bilinear)
	{
		if (texture == null)
		{
			texture = new Texture2D(width, 1);
		}
		else if (texture.width != width)
		{
			texture.Reinitialize(width, 1);
		}

		if (gradient == null)
		{
			gradient = new Gradient();
			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.black, 1) },
				new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
			);
		}

		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = filterMode;

		Color[] cols = new Color[width];
		for (int i = 0; i < cols.Length; i++)
		{
			float t = i / (cols.Length - 1f);
			cols[i] = gradient.Evaluate(t);
		}

		texture.SetPixels(cols);
		texture.Apply();
	}

	static readonly uint[] argsBufferArray = new uint[5];
	public static void CreateArgsBuffer(ref ComputeBuffer argsBuffer, Mesh mesh, int numInstances)
	{
		const int stride = sizeof(uint);
		const int numArgs = 5;
		const int subMeshIndex = 0;

		argsBuffer = new ComputeBuffer(numArgs, stride, ComputeBufferType.IndirectArguments);

		lock (argsBufferArray)
		{
			argsBufferArray[0] = (uint)mesh.GetIndexCount(subMeshIndex);
			argsBufferArray[1] = (uint)numInstances;
			argsBufferArray[2] = (uint)mesh.GetIndexStart(subMeshIndex);
			argsBufferArray[3] = (uint)mesh.GetBaseVertex(subMeshIndex);
			
			argsBuffer.SetData(argsBufferArray);
		}
	}
}
