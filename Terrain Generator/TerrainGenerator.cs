using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [Range(0, 24), Tooltip("The amount that the terrain mesh is simplified to. The higher the number, the lower the quality.")]
    public int meshSimplification = 0;
    [Tooltip("The size of the terrain.")]
    public int terrainSize = 241;
    [Tooltip("A boolean to auto update the terrain when changing settings instead of clicking the 'Generate' button.")]
    public bool autoUpdate = true;        
    [Tooltip("A toggle for use of a Falloff map. A Falloff map is a map used for generating islands in procedural terrain.")]
    public bool useFalloffMap = true;
    [Tooltip("A toggle to use a collider for the terrain mesh. Requires a MeshCollider component already on the terrain.")]
    public bool useMeshCollider = false;
    [Tooltip("A toggle to use the 32 bit mesh Index. By default procedural meshes in Unity use the 16 bit mesh index, which only allows for 65,535 vertices. The 32 bit mesh index allows for 4 billion vertices, at the expense of not being compatible with some platforms, and taking significantly more memory and bandwidth.")]
    public bool use32BitMeshIndex = false;
    [Tooltip("An integer to determine the actual seed of the terrain. Think of the seeds from minecraft when creating a new world and this is an implementation of that.")]
    public int seed;

    [Header("Noise Settings")]
    [Tooltip("The scale of the noise applied.")]
    public float noiseScale = 3.5f;
    [Tooltip("The offset in the x direction of the noise.")]
    public float noiseOffsetX = 0f;
    [Tooltip("The offset in the z direction of the noise.")]
    public float noiseOffsetZ = 0f;
    [Tooltip("The multiplier for the height of the terrain. Meant to be used in conjunction with the height Animation Curve.")]
    public float heightMultiplier = 33.7f;
    [Tooltip("An Animation Curve for editing the height of the terrain. This can be used to deliver mountains and canyons and valleys, and can be edited and tweaked to fit your needs.")]
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Advanced Noise Settings")]
    [Range(1, 6), Tooltip("The amount of layers of noise. More layers equals more detail.")]
    public int octaves = 4;
    [Tooltip("Controls the strength of each octave of noise.")]
    public float persistence = 0.25f;
    [Tooltip("Controls the frequency of each octave of noise.")]
    public float lacunarity = 3f;

    [Header("Falloff Settings")]
    [Tooltip("Where the falloff starts.")]
    public float falloffStart = 2.09f;
    [Tooltip("Where the falloff ends.")]
    public float falloffEnd = 2.84f;

    [Header("Visual Settings")]
    [Tooltip("A material for the terrain. In the material settings make sure to set the shader to 'TerrainShader'.")]
    public Material mat;

    [Header("Debugging")]
    [Tooltip("A toggle for printing the vertex count of the terrain mesh into the Unity console.")]
    public bool printVertexCount;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    private float[,] falloffMap;

    private float cachedTerrainSize;
    private float cachedFalloffStart;
    private float cachedFalloffEnd;

    private float seededOffsetX;
    private float seededOffsetZ;

    void Start()
    {
        // Creates the terrain mesh and gets the components required for generation
        CreateMeshAndGetComponents();

        // Generates the terrain
        GenerateTerrain();

        // Logs a warning to the console if no material is assigned to the mat section under "Visual Settings"
        if (mat == null)
            Debug.LogWarning("No Material found! Create a Material and drag it into the 'Mat' Slot!");
    }

    void CreateMeshAndGetComponents()
    {
        // Create the terrain mesh
        mesh = new Mesh();

        // Get the MeshFilter component
        meshFilter = GetComponent<MeshFilter>();

        // Get the MeshRenderer component
        meshRenderer = GetComponent<MeshRenderer>();

        // Assign the generated mesh to the MeshFilter
        meshFilter.mesh = mesh;        
    }

    void OnValidate()
    {
        if (!autoUpdate) return;

        // Generates the terrain
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        // If there is no mesh generated, then we will return out of this function
        if (mesh == null) return;

        // Handles the seed creation
        HandleSeedCreation();

        // Handles all the falloff optimization
        HandleFalloffCaching();

        // Handles all of the terrain settings
        HandleTerrainSettings();

        // Creates the terrain mesh
        CreateMesh();

        // Finalizes the terrain mesh
        FinalizeMesh();

        // Handles terrain collision
        HandleTerrainCollision();
    }

    void HandleSeedCreation()
    {
        // Creates a new System.Random in order to make seeds work
        System.Random prng = new System.Random(seed);
        seededOffsetX = noiseOffsetX + prng.Next(-100000, 100000);
        seededOffsetZ = noiseOffsetZ + prng.Next(-100000, 100000);        
    }

    void HandleFalloffCaching()
    {
        // Caches everything related to falloff generation
        if (falloffMap == null)
        {
            cachedTerrainSize = terrainSize;
            cachedFalloffStart = falloffStart;
            cachedFalloffEnd = falloffEnd;
        }

        // Checks if any of the parameters are not equal to the cached paremeters, if they are, we set the falloff map to nothing
        if (terrainSize != cachedTerrainSize || falloffStart != cachedFalloffStart || falloffEnd != cachedFalloffEnd)
            falloffMap = null;

        // If we want a falloff and the falloff map is equal to nothing, then we generate the falloff map
        if (useFalloffMap && falloffMap == null)
            falloffMap = GenerateFalloffMap(terrainSize);
    }

    void HandleTerrainSettings()
    {
        // Changes the index format of the mesh according to a boolean
        mesh.indexFormat = use32BitMeshIndex ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

        // Makes sure the terrain is always centered when changing the terrain size
        transform.position = new Vector3(-terrainSize / 2f, 0, -terrainSize / 2f);

        // Prints the vertex count of the terrain if needed
        if (printVertexCount)
            Debug.Log(mesh.vertexCount);

        // Set the material in the MeshRenderer to the material assigned under "Visual Settings"
        if (mat != null)
            meshRenderer.material = mat;        
    }

    void HandleTerrainCollision()
    {
        // If we want a collider and there isn't a mesh collider on the terrain gameObject
        if (useMeshCollider && GetComponent<MeshCollider>() == null)
        {
            // Prints a warning to the console 
            Debug.LogWarning("MeshCollider not found! Add a MeshCollider component for collision!");
        }
        // If we want a collider and there is a mesh collider on the terrain gameObject
        else if (useMeshCollider && GetComponent<MeshCollider>() != null)
        {
            // We get the meshCollider component and set its mesh equal to the generated terrain mesh
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        // If we don't want a collider and there is a mesh collider on the terrain gameObject
        else if (!useMeshCollider && GetComponent<MeshCollider>() != null)
        {
            // We set the mesh in the mesh collider component to nothing
            GetComponent<MeshCollider>().sharedMesh = null;
        }
    }

    void CreateMesh()
    {
        int meshSimplificationIncrement = meshSimplification == 0 ? 1 : meshSimplification * 2;
        int verticesPerLine = (terrainSize - 1) / meshSimplificationIncrement + 1;

        // Create the vertices
        vertices = new Vector3[(verticesPerLine + 1) * (verticesPerLine + 1)];
        uvs = new Vector2[vertices.Length];        

        // Loops through the terrain size 
        for (int i = 0, z = 0; z < terrainSize; z += meshSimplificationIncrement)
        {
            // Loops through the terrain size
            for (int x = 0; x < terrainSize; x += meshSimplificationIncrement)
            {
                float y = 0;

                // Loops through the number of octaves we want
                for (int o = 0; o < octaves; o++)
                {
                    float frequency = Mathf.Pow(lacunarity, o);
                    float ampltitude = Mathf.Pow(persistence, o);

                    // Assigns the y value to Mathf.PerlinNoise for some smooth randomness
                    y += Mathf.PerlinNoise((x + seededOffsetX) * (noiseScale / 200) * frequency, (z + seededOffsetZ) * (noiseScale / 200) * frequency) * ampltitude;
                }

                // If we want a falloff map
                if (useFalloffMap)
                {
                    // Gets the length of falloff map
                    int falloffX = Mathf.Clamp(x, 0, falloffMap.GetLength(0) - 1);
                    int falloffZ = Mathf.Clamp(z, 0, falloffMap.GetLength(1) - 1);
                    float falloff = falloffMap[falloffX, falloffZ];

                    // Applies the falloff map
                    y -= falloff;
                    y = Mathf.Clamp01(y);
                }

                // Applies the animation curve
                y *= heightCurve.Evaluate(y) * heightMultiplier;

                // Sets the vertices
                vertices[i] = new Vector3(x, y, z);

                int xIndex = x / meshSimplificationIncrement;
                int zIndex = z / meshSimplificationIncrement;

                // Sets the uvs, which are used for texture mapping
                uvs[i] = new Vector2(xIndex / (float)(verticesPerLine - 1), zIndex / (float)(verticesPerLine - 1));

                i++;
            }
        }

        // Create the triangles
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        int vert = 0;
        int tris = 0;

        // Loops through the vertices
        for (int z = 0; z < verticesPerLine - 1; z++)
        {
            for (int x = 0; x < verticesPerLine - 1; x++)
            {
                int topLeft = vert;
                int topRight = vert + 1;
                int bottomLeft = vert + verticesPerLine;
                int bottomRight = bottomLeft + 1;

                // Sets the triangles
                triangles[tris + 0] = topLeft;
                triangles[tris + 1] = bottomLeft;
                triangles[tris + 2] = topRight;

                triangles[tris + 3] = topRight;
                triangles[tris + 4] = bottomLeft;
                triangles[tris + 5] = bottomRight;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void FinalizeMesh()
    {
        // If there is no mesh generated, then we will return out of this function
        if (mesh == null) return;

        // Reset the mesh
        mesh.Clear();

        // Assign vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Assigns the calculated mesh uvs
        mesh.uv = uvs;        

        // Recalculate normals for correct lighting calculations
        mesh.RecalculateNormals();
    }

    public float[,] GenerateFalloffMap(int size)
    {
        // Creates a float array which will use the terrain size
        float[,] map = new float[size, size];

        float halfSize = size / 2;

        // Loops through the y and x
        for (int y = 0; y < size; y++)
        {
            float newY = Mathf.Abs(y - halfSize) / halfSize;

            for (int x = 0; x < size; x++)
            {
                // Does some math to calculate the falloff map
                float newX = Mathf.Abs(x - halfSize) / halfSize;

                float value = Mathf.Clamp01(Mathf.Max(newX, newY));
                map[x, y] = EvaluateFalloffMap(value);
            }
        }

        // Returns the map
        return map;
    }

    public float EvaluateFalloffMap(float value)
    {
        // Allows us to customize where the falloff map ends and starts
        float a = falloffStart;
        float b = falloffEnd;

        // Does some math to calculate the falloff map
        float powA = Mathf.Pow(value, a);
        float powB = Mathf.Pow(b - b * value, a);

        return powA / (powA + powB);
    }
}
