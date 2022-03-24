using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class World_GENERATOR : MonoBehaviour
{
	public const int chunkSize = 241;

	Queue<MapThreadInfo<NoiseData>> mapDataQueue = new Queue<MapThreadInfo<NoiseData>>();
	Queue<MapThreadInfo<MeshData>> meshDataQueue = new Queue<MapThreadInfo<MeshData>>();

	TerrainData terrainData;

	private void Awake()
    {
        terrainData = GetComponent<TerrainData>();
    }

	public void RequestHeightMapData(Vector2 center, Action<NoiseData> callback)
	{

		ThreadStart threadStart = delegate {
			HeightMapThread( center, callback);
		};

		new Thread(threadStart).Start();
	}

		void HeightMapThread(Vector2 center, Action<NoiseData> callback)
		{
			NoiseData noiseMap = Noise_GENERATOR.GenerateNoise(chunkSize, terrainData.numberOfOctaves, terrainData.terrainSeed, terrainData.noiseScale, terrainData.persistence, terrainData.lacunarity, center);
			lock (mapDataQueue)
			{
				mapDataQueue.Enqueue(new MapThreadInfo<NoiseData>(callback, noiseMap));
			}
		}

	public void RequestMeshData(NoiseData noiseData,int meshLOD, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate {
			MeshDataThread(noiseData, meshLOD, callback);
		};

		new Thread(threadStart).Start();
	}

		void MeshDataThread(NoiseData noiseData,int meshLOD, Action<MeshData> callback)
		{

			MeshData meshData = Mesh_GENERATOR.GenerateChunkMesh(chunkSize, noiseData.noiseMap, terrainData.terrainHeight, terrainData.terrainCurve, meshLOD);
			lock (meshDataQueue)
			{
				meshDataQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
			}
		}

	void Update()
	{
		while (mapDataQueue.Count > 0)
        {
			MapThreadInfo<NoiseData> threadInfo = mapDataQueue.Dequeue();
			threadInfo.callback(threadInfo.parameter);
		}

		while (meshDataQueue.Count > 0)
		{
			MapThreadInfo<MeshData> threadInfo = meshDataQueue.Dequeue();
			threadInfo.callback(threadInfo.parameter);
		}
	}

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}

	}

}
