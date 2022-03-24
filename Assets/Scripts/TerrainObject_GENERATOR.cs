using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class TerrainObject_GENERATOR: MonoBehaviour
{
    List<GameObject> spawnedObjects = new List<GameObject>();
    List<Vector3> usedPositions = new List<Vector3>();

    Transform viewer;
    int renderRange;


    private void Update()
    {

        if (viewer == null) { return; }

        Vector2 currentViewerPosition = new Vector2(viewer.transform.position.x, viewer.transform.position.z);

        //Enable objects depending on cameraDistance
        foreach (GameObject _object in spawnedObjects)
        {
            Vector2 treePosition = new Vector2(_object.transform.position.x, _object.transform.position.z);

            if ((currentViewerPosition - treePosition).magnitude < renderRange){
                _object.SetActive(true);
            }else{
                _object.SetActive(false);
            }
        }
    }

    public void Generate(int chunkSize,
                         float[,] noiseMap,
                         Vector3[] vertices,
                         Vector3[] meshNormals,
                         int objectDensity,
                         Transform viewer,
                         GameObject[] spawnableObjects,
                         int renderRange,
                         int seed,
                         Vector3 objectPosition)
    {
        //SET DATA
        this.renderRange = renderRange;
        this.viewer = viewer;

        float maxSlopeAngle = 45;
        float waterLevel = 0.4f;
        float nonSpawnableHeight = 90;
        float spawnableDistanceFromWater = 10;

        System.Random rand = new System.Random(seed.GetHashCode());
        GameObject objectHolder = new GameObject("Forest");
        objectHolder.isStatic = true;
        objectHolder.transform.parent = transform;

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
        while (spawnedObjects.Count < numberOfTrees)
        {
            int x = rand.Next(0, chunkSize-1 );
            int z = rand.Next(0, chunkSize-1 );
            int i = z * chunkSize + x;
            float y = vertices[z * (chunkSize-1) + x].y;

            Vector3 meshNormal = meshNormals[z * (chunkSize-1) + x];
            float normalAngle = Vector3.Angle(meshNormal, new Vector3(0, 1, 0));

            if (((y > (waterLevel + spawnableDistanceFromWater) && (y < nonSpawnableHeight)) && normalAngle < maxSlopeAngle) && vertices[i].y > 10f)
            {
                Vector3 position = new Vector3(vertices[i].x + objectPosition.x, vertices[i].y, vertices[i].z + objectPosition.z);
                if (!usedPositions.Contains(position))
                {
                    usedPositions.Add(position);
                    GameObject treeToSpawn = spawnableObjects[rand.Next(0, spawnableObjects.Length)];

                    Quaternion objectRotation = Quaternion.Euler(new Vector3(0, Random.Range(0f, 360f), 0));
                    GameObject newObject = Instantiate(treeToSpawn, position, objectRotation, objectHolder.transform);
                    newObject.isStatic = true;
                    newObject.SetActive(false);
                    spawnedObjects.Add(newObject);

                }
            }
        }

    }
}

