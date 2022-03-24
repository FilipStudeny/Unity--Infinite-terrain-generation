using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
public class InfiniteTerrain_GENERATOR : MonoBehaviour
{
	
	//Viewer Data
	const float viewerNiveDistanceBeforeChunkUpdated = 25f;
	const float sqrVieverChunkUpdate = viewerNiveDistanceBeforeChunkUpdated * viewerNiveDistanceBeforeChunkUpdated;

	static int maxRenderDistance;
	static int minRenderDistance;
	static Vector2 viewerPosition;
	Vector2 oldViewerPosition;

	int chunksVisibleInViewDst;

	static World_GENERATOR worldGenerator;
	static TerrainData terrainData;

	//Terrain Data
	public Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
	public List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Awake()
	{
		terrainData = GetComponent<TerrainData>();
		worldGenerator = FindObjectOfType<World_GENERATOR>();
	}
    private void Start()
    {
		Generate();
    }

    public void Generate()
    {
		SwitchRenderDistance();
	}

	public void SwitchRenderDistance()
    {

		maxRenderDistance = terrainData.renderDistance.meshLODs[terrainData.renderDistance.meshLODs.Length - 1].distanceFromViewer;
		minRenderDistance = terrainData.renderDistance.meshLODs[1].distanceFromViewer;
		terrainData.userCamera.GetComponent<Camera>().farClipPlane = maxRenderDistance;

		chunksVisibleInViewDst = Mathf.RoundToInt(maxRenderDistance / 240);
		UpdateVisibleChunks();

	}

	void Update()
	{
		viewerPosition = new Vector2(terrainData.userCamera.position.x, terrainData.userCamera.position.z);

		if((oldViewerPosition - viewerPosition).sqrMagnitude > sqrVieverChunkUpdate){
			oldViewerPosition = viewerPosition;
			UpdateVisibleChunks();
        }
	}
	void UpdateVisibleChunks()
	{

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
		{
			terrainChunksVisibleLastUpdate[i].SetVisible(false);
		}
		terrainChunksVisibleLastUpdate.Clear();

		//Get chunk Coord
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / 240);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / 240);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunks.ContainsKey(viewedChunkCoord))
				{
					terrainChunks[viewedChunkCoord].UpdateTerrainChunk();	
				}else{

					terrainChunks.Add(viewedChunkCoord,
						new TerrainChunk(viewedChunkCoord, 240, transform, terrainChunksVisibleLastUpdate)
						);
				}
			}
		}
	}


	public class TerrainChunk
	{
		GameObject meshObject;
		Material terrainMaterial;
		MeshLOD[] meshLODs;
		NoiseData noiseData;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		Vector2 position;
		Vector3 positionV3;

		Bounds bounds;
		MeshLODLevel[] meshLODlevels;
		
		bool mapDataReceived;
		int previousLODlevel = -1;
		public bool hasGeneratedObjects = false;

		List<TerrainChunk> terrainChunksVisibleLastUpdate;
		TerrainObject_GENERATOR terrainObject_GENERATOR;

		public TerrainChunk(Vector2 coord, int chunkSize, Transform parent, List<TerrainChunk> terrainChunksVisibleLastUpdate)
		{
			this.meshLODlevels = terrainData.renderDistance.meshLODs;
			this.terrainChunksVisibleLastUpdate = terrainChunksVisibleLastUpdate;

			position = coord * chunkSize;
			bounds = new Bounds(position, Vector2.one * chunkSize);
			positionV3 = new Vector3(position.x + (coord.x + 0.5f), 0, position.y + (coord.y + 0.5f));

			meshObject = new GameObject("Terrain Chunk");
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer = meshObject.AddComponent<MeshRenderer>();

			terrainMaterial = terrainData.terrainMaterial;
			terrainMaterial.enableInstancing = true;

			meshRenderer.material = terrainMaterial;
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;

            if (terrainData.generateObjects){
				terrainObject_GENERATOR = meshObject.AddComponent<TerrainObject_GENERATOR>();
            }

			meshLODs = new MeshLOD[meshLODlevels.Length];
            for (int i = 0; i < meshLODs.Length; i++){
				meshLODs[i] = new MeshLOD(meshLODlevels[i].LODlevel, UpdateTerrainChunk);
			}

			worldGenerator.RequestHeightMapData(position,OnNoiseDataReceived);
			SetVisible(false);

		}

		void OnNoiseDataReceived(NoiseData noiseData)
		{
			this.noiseData = noiseData;
			mapDataReceived = true;

			UpdateTerrainChunk();
		}

		public void UpdateTerrainChunk()
		{
            if (mapDataReceived)
            {
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxRenderDistance;

				if (visible)
                {
					int lodIndex = 0;
					for (int i = 0; i < meshLODlevels.Length - 1; i++)
                    {
						if(viewerDstFromNearestEdge > meshLODlevels[i].distanceFromViewer){
							lodIndex = i + 1;
                        }else { 
							break; 
						}
                    }

					if (lodIndex != previousLODlevel)
                    {
						MeshLOD meshLOD = meshLODs[lodIndex];
                        if (meshLOD.hasReceivedMesh)
                        {
							previousLODlevel = lodIndex;
							meshFilter.mesh = meshLOD.mesh;
						
						}
						else if (!meshLOD.hasReceivedMesh)
                        {
							meshLOD.RequestMeshData(noiseData);
                        }
                    }

					terrainChunksVisibleLastUpdate.Add(this);

                    if (terrainData.generateObjects && !hasGeneratedObjects)
                    {
                        if (meshLODs[0].hasReceivedMesh && viewerDstFromNearestEdge <= meshLODlevels[0].distanceFromViewer)
                        {
							Vector3[] vertices = meshLODs[0].vertPositions;
							Vector3[] normals = meshFilter.sharedMesh.normals;
							float[,] noiseMap = meshLODs[0].noiseMap;
							int chunkSize = meshLODs[0].chunkSize;

							terrainObject_GENERATOR.Generate(chunkSize, noiseMap, vertices, normals, terrainData.objectDensity, terrainData.userCamera, terrainData.spawnableObjects, minRenderDistance, terrainData.terrainSeed, positionV3);
							hasGeneratedObjects = true;

						}
                    }
				}
				SetVisible(visible);
			}
		}

		public void SetVisible(bool visible)
		{
			meshObject.SetActive(visible);
		}

		public bool IsVisible()
		{
			return meshObject.activeSelf;
		}
	}

	class MeshLOD
    {
		public Mesh mesh;
		public bool hasRequestedMeshData;
		public bool hasReceivedMesh;
		public float[,] noiseMap;
		public Vector3[] vertPositions;
		public int chunkSize;

		int meshLOD;
		System.Action updateLODCallback;

		public MeshLOD(int meshLOD, System.Action updateLODCallback)
        {
			this.meshLOD = meshLOD;
			this.updateLODCallback = updateLODCallback;
        }

		void OnMeshDataReceived(MeshData meshData)
        {
			mesh = meshData.CreateMesh();
			vertPositions = meshData.vertPositions;
			chunkSize = meshData.chunkSize;
			hasReceivedMesh = true;

			updateLODCallback();
        }

		public void RequestMeshData(NoiseData noiseData)
        {
			hasRequestedMeshData = true;
			worldGenerator.RequestMeshData(noiseData, meshLOD, OnMeshDataReceived);
			noiseMap = noiseData.noiseMap;
        }
    }
}


