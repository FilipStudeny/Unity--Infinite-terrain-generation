using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  static class Mesh_GENERATOR
{


    public static MeshData GenerateChunkMesh(int chunkSize,float[,] noiseMapData,float elevationScale,AnimationCurve terrainCurve ,int LODlevel )
    {
        int levelOfDetailIncrement = (LODlevel == 0) ? 1 : LODlevel * 2;
        int numberOfVerticesPerRow = (chunkSize - 1 ) / levelOfDetailIncrement +1;
        float[,] noiseMap = noiseMapData;
        AnimationCurve heightCurve = new AnimationCurve(terrainCurve.keys);

        //Setup variables
        Vector3[] vertices = new Vector3[numberOfVerticesPerRow * numberOfVerticesPerRow];
        int[] triangles = new int[(numberOfVerticesPerRow - 1) * (numberOfVerticesPerRow - 1) * 6];
        Vector2[] uvs = new Vector2[numberOfVerticesPerRow * numberOfVerticesPerRow];
        Vector3[] vertPositions = new Vector3[numberOfVerticesPerRow * numberOfVerticesPerRow];

        MeshData meshData = new MeshData(numberOfVerticesPerRow);
        meshData.SetFields(vertices, uvs, triangles,vertPositions);

        
        int vertexIndex = 0;
        for (int y = 0; y < chunkSize; y+= levelOfDetailIncrement)
        {
            for (int x = 0; x < chunkSize; x+= levelOfDetailIncrement)
            {

                //Create vertices at position and center mesh
                float noiseSample = noiseMap[x, y];
                float height = heightCurve.Evaluate(noiseSample) * elevationScale;
                Vector2 percentPosition = new Vector2(x / (chunkSize - 1f), y / (chunkSize -1f ));
                Vector3 vertPosition = new Vector3(percentPosition.x * 2 - 1, 0, percentPosition.y * 2 - 1) * chunkSize/2;
                vertPosition.y = height;
                vertices[vertexIndex] = vertPosition;
                vertPositions[vertexIndex] = vertPosition;



                //UVS calculation
                uvs[vertexIndex] = new Vector2((float)x / numberOfVerticesPerRow, (float)y / numberOfVerticesPerRow);


                meshData.AddToFields(vertices, uvs,vertPositions,vertexIndex);

                //Construct triangles
                if (x != chunkSize - 1 && y != chunkSize - 1)
                {
                    meshData.ConstructTriangles(vertexIndex + numberOfVerticesPerRow, vertexIndex + numberOfVerticesPerRow + 1, vertexIndex);
                    meshData.ConstructTriangles(vertexIndex + numberOfVerticesPerRow + 1, vertexIndex + 1, vertexIndex);
                    
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
	public int chunkSize;
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;
    public Vector3[] vertPositions;

    public Mesh mesh;
    int triangleIndex;
	public MeshData(int chunkSize )
    {
		this.chunkSize = chunkSize;
        
    }

    public void ConstructTriangles(int a,int b, int c)
    {
        triangles[triangleIndex + 0] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public void SetFields(Vector3[] vertices, Vector2[] uvs,int[] triangles,Vector3[] vertPositions)
    {
        this.vertices = vertices;
        this.uvs = uvs;
        this.triangles = triangles;
        this.vertPositions = vertPositions;
    }

    public void AddToFields(Vector3[] vertices,Vector2[] uvs,Vector3[] vertPositions, int i)
    {
        this.vertices[i] = vertices[i];
        this.uvs[i] = uvs[i];
        this.vertPositions[i] = vertPositions[i];
    }

    public Mesh CreateMesh()
    {
       if(mesh == null) { mesh = new Mesh(); } else { mesh.Clear(); }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();


        return mesh;
    }
}