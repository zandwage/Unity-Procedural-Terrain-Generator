using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Gets the terrain generator script
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        // Makes a custom inspector
        base.OnInspectorGUI();

        // Creates a button in the inspector to generate the terrain
        if (GUILayout.Button("Generate Terrain"))
        {
            terrainGenerator.GenerateTerrain();
        }
    }
}
