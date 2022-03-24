using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MeshLODLevel
{
	public int LODlevel;
	public int distanceFromViewer;
}

[CreateAssetMenu()]
public class RenderDistanceLODLevels : ScriptableObject
{
	public MeshLODLevel[] meshLODs;
}
