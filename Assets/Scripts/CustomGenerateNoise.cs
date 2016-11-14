using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class CustomGenerateNoise : Editor
{

    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGen = (TerrainGenerator)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            terrainGen.GenerateTerrain();
        }
    }
}
