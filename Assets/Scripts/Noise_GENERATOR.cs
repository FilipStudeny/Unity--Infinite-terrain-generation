using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class Noise_GENERATOR
{
	public static NoiseData GenerateNoise(int chunkSize, int octaves, int seed, int noiseScale, float persistence, float lacunarity, Vector2 offset)
	{

		float[,] noiseMap = new float[chunkSize, chunkSize];

		System.Random prng = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistence;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = chunkSize / 2f;
		float halfHeight = chunkSize / 2f;

		for (int y = 0; y < chunkSize; y++)
		{
			for (int x = 0; x < chunkSize; x++)
			{

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++)
				{
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / noiseScale * frequency;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / noiseScale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistence;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight)
				{
					maxLocalNoiseHeight = noiseHeight;
				}
				else if (noiseHeight < minLocalNoiseHeight)
				{
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap[x, y] = noiseHeight;

				float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
				noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
			}
		}

		NoiseData noiseData = new NoiseData(noiseMap);
		return noiseData;

	}
}
public class NoiseData
{
	public float[,] noiseMap;

	public NoiseData(float[,] noiseMap)
    {
		this.noiseMap = noiseMap;
    }
}


