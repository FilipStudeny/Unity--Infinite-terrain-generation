using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    public List<GameObject> trees;
    List<Vector3> usedPositions = new List<Vector3>();

    public Transform viewer;
    public GameObject[] spawnableObjects;
    public int renderRange;

    public float waterLevel;
    public int spawnableDistanceFromWater;
    public int nonSpawnableHeight;
    public int maxSlopeAngle;
    public int objectDensity;
    public int seed;
 
    public Vector3 objectPosition;

    public int chunkSize;
    public float[,] noiseMap;
    public Vector3[] vertices;
    public Vector3[] meshNormals;
    public bool hasGeneratedForest;

    public void InitializeForest(int chunkSize,
                                float[,] noiseMap,
                                Vector3[] vertices,
                                Vector3[] meshNormals,
                                float waterLevel,
                                int spawnableDistanceFromWater,
                                int nonSpawnableHeight,
                                int objectDensity,
                                int maxSlopeAngle,
                                Transform viewer,
                                GameObject[] spawnableObjects,
                                int renderRange,
                                int seed,
                                Vector3 objectPosition)
   
    {
        this.chunkSize = chunkSize;
        this.noiseMap = noiseMap;
        this.vertices = vertices;
        this.meshNormals = meshNormals;

        this.waterLevel = waterLevel;
        this.objectDensity = objectDensity;
        this.maxSlopeAngle = maxSlopeAngle;
        this.nonSpawnableHeight = nonSpawnableHeight;
        this.spawnableDistanceFromWater = spawnableDistanceFromWater;
        this.renderRange = renderRange;
        this.viewer = viewer;
        this.spawnableObjects = spawnableObjects;
        this.seed = seed;
        this.objectPosition = objectPosition;
        hasGeneratedForest = false;
        trees = new List<GameObject>();
        

    }

   
    private void Update()
    {
        
            if (viewer == null) { return; }

            Vector2 currentViewerPosition = new Vector2(viewer.transform.position.x, viewer.transform.position.z);

            //Enable objects depending on cameraDistance
            foreach (GameObject tree in trees)
            {
                Vector2 treePosition = new Vector2(tree.transform.position.x, tree.transform.position.z);

                if ((currentViewerPosition - treePosition).magnitude < renderRange)
                {
                    tree.SetActive(true);
                }
                else
                {
                    tree.SetActive(false);
                }
            }
        

    }
    public void Generate()
    {
    

  

        System.Random rand = new System.Random(seed.GetHashCode());
        GameObject forestGameObject = new GameObject("Forest");
        forestGameObject.transform.parent = transform;
        

        //Get are above water(number of vertices)
        int areaAboveWater = 0;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                if (noiseMap[x, y] > waterLevel)
                {
                    areaAboveWater++;
                }
            }
        }

        int numberOfTrees = (int)((areaAboveWater / 1000f) * objectDensity);

        while (trees.Count < numberOfTrees)
        {
            int x = rand.Next(0, chunkSize - 1);
            int z = rand.Next(0, chunkSize - 1);
            int i = z * chunkSize + x;
            float y = vertices[z * chunkSize + x].y;

            Vector3 meshNormal = meshNormals[z * chunkSize + x];
            float normalAngle = Vector3.Angle(meshNormal, new Vector3(0, 1, 0));

            if ((y > (waterLevel + spawnableDistanceFromWater) && (y < nonSpawnableHeight)) && normalAngle < maxSlopeAngle)
            {
                
                Vector3 position = new Vector3(vertices[i].x + objectPosition.x, vertices[i].y, vertices[i].z + objectPosition.z);

                if (!usedPositions.Contains(position))
                {
                    usedPositions.Add(position);
                    GameObject treeToSpawn = spawnableObjects[rand.Next(0, spawnableObjects.Length)];

                    Quaternion objectRotation = Quaternion.Euler(new Vector3(0, Random.Range(0f, 360f), 0));
                    GameObject tree = Instantiate(treeToSpawn, position, objectRotation, forestGameObject.transform);
                    // tree.transform.localScale = new Vector3(1, 1, 1);
                    ///  tree.layer = LayerMask.NameToLayer("Ground");

                    tree.SetActive(false);
                    trees.Add(tree);
                }
                
            }
        }


    }


}


