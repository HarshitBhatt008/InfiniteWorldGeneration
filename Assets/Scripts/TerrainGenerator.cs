using System.Collections.Generic;
using UnityEngine;

/// <summary>
    ///The TerrainGenerator script is responsible for dynamically generating and managing terrains in a Unity environment. 
    ///It utilizes Perlin noise to create realistic terrain features and optimizes terrain loading based on the player's 
    ///position.

    ///Public Variables
    ///terrainSize: The size of each terrain in Unity units.
    ///terrainRingSize: The number of terrain rings around the player.
    ///frequency: Frequency of Perlin noise for terrain generation.
    ///amplitude: Amplitude of Perlin noise for terrain generation.
    ///viewDistance: The distance at which terrains are considered for loading.
    ///ChunkItemPrefabs: An array of GameObject prefabs representing items to be scattered on the terrain.


    /// Important Methods:
    ///Start(): Initializes the terrain generator, creating the root object and setting initial values.

    ///Update(): Updates the terrain based on the player's position, activating and deactivating terrains as needed.

    ///UpdateTerrainObject(): Updates the terrain objects based on the player's position, loading and unloading terrains dynamically.

    ///ChunkObjectGenerator(Transform chunkParent, List<Vector3> verticesPos): Generates chunk items on the terrain at random positions.

    /// Important Methods(Sub class): NoiseGenerator
    ///Noise(float x, float y): Generates Perlin noise based on the specified frequency and amplitude.

    /// Important Methods(Sub class): TerrainObject
    ///TerrainObject(Vector2 centrePos, int size, Transform root, NoiseGenerator noiseGenerator): Initializes a terrain object 
    ///with a specified center position, size, root transform, and noise generator.

    ///UpdateTerrainObject(Vector3 currPos, int viewDistance): Updates the terrain object based on the player's position, 
    ///activating or deactivating it accordingly.

    ///IsActive(): Returns whether the terrain object is currently active.

/// </summary>




public class TerrainGenerator : MonoBehaviour
{
    public int terrainSize = 100;
    [Range(2, 100)]
    public int terrainRingSize = 2;
    public float frequency = 8.0f;
    public int amplitude = 10;
    public const int viewDistance = 200;

    [Header("Chunk Items")]
    public GameObject[] ChunkItemPrefabs;


    Transform terrainRoot;
    Transform transformComponent;
    Vector3 currPosition;
    int numTerrainsWidth;
    Dictionary<Vector2, TerrainObject> loadedTerrains = new Dictionary<Vector2, TerrainObject>();
    Dictionary<Vector2, TerrainObject> unloadedTerrains = new Dictionary<Vector2, TerrainObject>();
    NoiseGenerator noiseGenerator;

    void Start()
    {
        terrainRoot = new GameObject("Terrain Root").transform;
        transformComponent = GetComponent<Transform>();
        currPosition = transformComponent.position;
        numTerrainsWidth = terrainRingSize * 2 - 1;
        noiseGenerator = new NoiseGenerator(frequency, amplitude);
        UpdateTerrainObject();
    }

    void Update()
    {
        currPosition = transformComponent.position;
        foreach (var pair in loadedTerrains)
        {
            pair.Value.UpdateTerrainObject(currPosition, viewDistance);
        }
        foreach (var pair in unloadedTerrains)
        {
            pair.Value.UpdateTerrainObject(currPosition, viewDistance);
        }
        UpdateTerrainObject();
    }

    void UpdateTerrainObject()
    {
        int currTerrainX = Mathf.RoundToInt(currPosition.x / terrainSize);
        int currTerrainZ = Mathf.RoundToInt(currPosition.z / terrainSize);

        for (int i = -numTerrainsWidth/2; i <= numTerrainsWidth/2; ++i)
        {
            for (int j = -numTerrainsWidth/2; j <= numTerrainsWidth/2; ++j)
            {
                Vector2 terrainCoords = new Vector2(currTerrainX + i, currTerrainZ + j);
                if (unloadedTerrains.ContainsKey(terrainCoords))
                {
                    if (unloadedTerrains[terrainCoords].IsActive())
                    {
                        loadedTerrains.Add(terrainCoords, unloadedTerrains[terrainCoords]);
                        unloadedTerrains.Remove(terrainCoords);
                    }
                }
                else if (loadedTerrains.ContainsKey(terrainCoords))
                {
                    if (!loadedTerrains[terrainCoords].IsActive())
                    {
                        unloadedTerrains.Add(terrainCoords, loadedTerrains[terrainCoords]);
                        loadedTerrains.Remove(terrainCoords);
                    }
                }
                else
                {
                    Vector2 terrainCentrePos = new Vector2(terrainCoords.x * terrainSize, terrainCoords.y * terrainSize);
                    TerrainObject newTerrain = new TerrainObject(terrainCentrePos, terrainSize, terrainRoot, noiseGenerator);
                    newTerrain.UpdateTerrainObject(currPosition, viewDistance);
                    if (newTerrain.IsActive())
                    {
                        loadedTerrains.Add(terrainCoords, newTerrain);
                        ChunkObjectGenerator(newTerrain.terrainObj.transform, newTerrain.verticesPos);
                    }
                    else
                    {
                        unloadedTerrains.Add(terrainCoords, newTerrain);
                    }
                }
            }
        }
    }

    public void ChunkObjectGenerator(Transform chunkParent, List<Vector3> verticesPos)
    {
        int chunkItemIndex = Random.Range(4, 6);
        for (int i = 0; i < chunkItemIndex; i++)
        {
            Vector3 chunkRandomPos = verticesPos[Random.Range(0, verticesPos.Count)];
            GameObject itemPrefab = ChunkItemPrefabs[Random.Range(0, ChunkItemPrefabs.Length)];
            GameObject chunkItem = Instantiate(itemPrefab, chunkParent);
            chunkItem.transform.localScale = new Vector3(3, 3, 3);
            if (chunkItem.tag == "rock")
            {
                chunkItem.transform.localPosition = new Vector3(chunkRandomPos.x, chunkRandomPos.y - .1f, chunkRandomPos.z);
            }
            else
            {
                chunkItem.transform.localPosition = new Vector3(chunkRandomPos.x, chunkRandomPos.y + 1f, chunkRandomPos.z);
            }
        }
    }

    public class NoiseGenerator
    {
        float frequency;
        int amplitude;

        public NoiseGenerator(float frequency, int amplitude)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
        }

        public float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x / frequency, y / frequency) * amplitude;
        }
    }

    public class TerrainObject
    {
        public GameObject terrainObj;
        public List<Vector3> verticesPos = new List<Vector3>();
        GameObject terrainObject;
        Bounds terrainBounds;

        public TerrainObject(Vector2 centrePos, int size, Transform root, NoiseGenerator noiseGenerator)
        {
            GenerateMesh(centrePos, size, root, noiseGenerator);
        }

        void GenerateMesh(Vector2 centrePos, int size, Transform root, NoiseGenerator noiseGenerator)
        {
            // Create vertices
            Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
            for (int i = -size / 2, k = 0 ; i <= size/2; ++i)
            {
                for (int j = -size/2; j <= size/2; ++j)
                {
                    vertices[k] = new Vector3(j, noiseGenerator.Noise(centrePos.x + j, centrePos.y + i), i);
                    verticesPos.Add(vertices[k]);
                    ++k;
                }
            }

            // Create triangles
            int[] triangles = new int[size * size * 2 * 3];
            for (int i = 0, bottomLeft = 0, k = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    // Top left triangle
                    triangles[k] = bottomLeft;
                    triangles[k + 1] = bottomLeft + size + 1;
                    triangles[k + 2] = bottomLeft + size + 2;
                    // Bottom right triangle
                    triangles[k + 3] = bottomLeft;
                    triangles[k + 4] = bottomLeft + size + 2;
                    triangles[k + 5] = bottomLeft + 1;
                    k += 6;
                    ++bottomLeft;
                }
                ++bottomLeft;
            }

            // Create Terrain
            terrainObject = new GameObject("Terrain");
            terrainObject.SetActive(false);
            terrainObject.GetComponent<Transform>().SetParent(root);
            terrainObject.AddComponent<MeshFilter>();
            terrainObject.AddComponent<MeshRenderer>();
            terrainObject.AddComponent<MeshCollider>();

            terrainBounds = new Bounds(new Vector3(centrePos.x, 0, centrePos.y), Vector3.one * size);

            // Set transform and mesh
            terrainObject.transform.position = new Vector3(centrePos.x, 0, centrePos.y);
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            terrainObject.GetComponent<MeshFilter>().mesh = mesh;
            terrainObject.GetComponent<MeshCollider>().sharedMesh = mesh;

            // Set material
            terrainObject.GetComponent<MeshRenderer>().materials[0] = Resources.Load<Material>("New Material");
            terrainObject.GetComponent<MeshRenderer>().materials[0].color = new Color(.26f, 1, 0, 1);
            terrainObj = terrainObject;

        }

        public void UpdateTerrainObject(Vector3 currPos, int viewDistance)
        {
            //Debug.Log(Mathf.Sqrt(chunkBounds.SqrDistance(currPos)));
            SetActive(Mathf.Sqrt(terrainBounds.SqrDistance(currPos)) <= viewDistance);
        }

        public bool IsActive()
        {
            return terrainObject.activeSelf;
        }

        void SetActive(bool value)
        {
            terrainObject.SetActive(value);
        }
    }
}