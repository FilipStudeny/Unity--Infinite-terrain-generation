using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainData : MonoBehaviour
{
    [Header("Terrain Data")]
    public int terrainSeed;
    public int terrainHeight;
    public bool generateWater;
    public bool generateClouds;
    public bool generateObjects;
    public int objectDensity;
    public AnimationCurve terrainCurve;
    public Material terrainMaterial;

    [Header("Noise Data")]
    public int numberOfOctaves;
    public int noiseScale;
    public float persistence;
    public float lacunarity;

    [Header("Miscelanious")]
    public Transform userCamera;
    
    [Header("Render settings")]
    public RenderDistanceLODLevels renderDistance;

    [Header("Objects")]
    public GameObject[] spawnableObjects;


    InfiniteTerrain_GENERATOR infiniteTerrainGENERATOR;

    private void Awake()
    {
        infiniteTerrainGENERATOR = GetComponent<InfiniteTerrain_GENERATOR>();   
    }

    void DestroyChildren()
    {
        infiniteTerrainGENERATOR.terrainChunks.Clear();
        infiniteTerrainGENERATOR.terrainChunksVisibleLastUpdate.Clear();
        while (gameObject.transform.childCount > 0)
        {

            DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
        }
    }
}
