using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh }
    public DrawMode drawMode;

    //MeshFilter and Render for the mesh
    public MeshFilter meshFilter;
    public MeshRenderer meshRender;

    //Texure for the mesh
    public Texture2D meshTex;

    //Width of the terrain
    public int width;
    //height(length) of the terrain
    public int height;


    //Height map vertices 
    private Vector3[] _vertices;
    //Height map normals
    private Vector3[] _normals;
    //Height map uv coords
    private Vector2[] _uvs;
    //Height map trinagles
    private int[] _triangles;
    //Height map indicies
    int numIndicies;

    //Noise map vertices
    private Vector3[] vertices;
    //Noise map normals
    private Vector3[] normals;
    //Noise map uvs
    private Vector2[] uvs;
    //Noise map triangles
    private int[] triangles;

    //Bool for if height map is true
    public bool heightMap = false;
    //Height map min height
    public float minTerrainHeight = 0f;
    //Hieght map max height
    public float maxTerrainHeight = 0f;

    //Height map texture
    private Texture2D tex;

    //seed value for the mesh 
    public int seed = 100;

    //scale fro the perlin noise
    public float scale = 27.0f;

    //height multiplier for noise map so the terrain is not flat
    public float heightMultiplier = 20.0f;

    //AnimationCurve for the height on the mesh from perlin noise
    public AnimationCurve heightCurve;

    //regions for the mesh depending on the height
    public Regions[] regions;

    //Float used to store the height value of the pixel(vertex)
    float currentHeight;

    //2D float array for the noise values
    float[,] noiseMap;

    public void GenerateTerrain()
    {
        //Load in the heightMap from the resources folder
        tex = LoadPNG("Assets/Resources/heightMap.png");


        //used in height map

        int iTotalVertices = (width + 1) * (height + 1);
        _vertices = new Vector3[iTotalVertices];

        _normals = new Vector3[iTotalVertices];

        _uvs = new Vector2[iTotalVertices];

        numIndicies = height * width * 6;

        _triangles = new int[numIndicies];

        //Noise map
        vertices = new Vector3[width * height];

        normals = new Vector3[width * height];

        uvs = new Vector2[width * height];

        //Generate the noise map
        noiseMap = PerlinNoiseGenerator.GenerateNoise(width, height, scale, seed);

        //Create new texture for the mesh
        meshTex = new Texture2D(width, height);

        Generate();
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    private void Generate()
    {
        if(heightMap)
        {
            GenerateMeshHeightMap();
        }
        else
        {
            GenerateMeshNoise();
        }

        //GenerateMeshNoise();
    }

    //Function for using a height map
    private void GenerateMeshHeightMap()
    {
        //Crate a new mesh
        Mesh _mesh = new Mesh();;

        //set uvIndex to 0
        int uvIndex = 0;

        //
        float xRatio = tex.width / width;
        float yRatio = tex.height / height;

       //
        int x = 0, z = 0;
        for (int i = 0; i < _vertices.Length; ++i)
        {
            //Get the curretn pixel colour from the height map
            Color pix = tex.GetPixel((int)(x * xRatio), (int)(z * yRatio));
            //Assign a float value from the gray scale value of the height map pixel
            float zVal = minTerrainHeight + maxTerrainHeight * pix.grayscale;
        
            //Set the vertex position
            _vertices[i].Set(x, zVal, z);
        
            //Set the uv coord
            _uvs[i].Set(x / (float)width, z / (float)height);

            //Increment uvIndex by 1
            uvIndex++;
            //Increment X by 1
            ++x;

            //get the remainder of x divide by height plus 1
            if (x % (height + 1) == 0)
            {
                x = 0;
                ++z;
            }
        }
        

        for (int ti = 0, vi = 0; vi < (height * (width + 1)); ++vi, ti += 6)
        {

            _triangles[ti] = vi;
            _triangles[ti + 3] = _triangles[ti + 2] = vi + 1;
            _triangles[ti + 4] = _triangles[ti + 1] = vi + height + 1;
            _triangles[ti + 5] = vi + height + 2;
        
            if (vi != 0 && (vi + 2) % (height + 1) == 0)
            {
                ++vi;
            }
        }
        
        //Go though each pixel 
        for (int yi = 0; yi < height + 1; ++yi) 
        {
            for (int xi = 0; xi < width + 1; ++xi)
            {
                //Store the pixel height from the gray scale
                currentHeight = tex.GetPixel(xi, yi).grayscale;
        
                for (int i = 0; i < regions.Length; ++i)
                {
                    //If the currentHeight is less than or equal too the current region height value 
                    //colour the pixel to the corresponding region
                    if (currentHeight <= regions[i].height)
                    {
                        meshTex.SetPixel(xi, yi, regions[i].colour);
                        break;
                    }
                }
            }
        }

        //Apply the texture data to the texture
        meshTex.Apply();

        //Set the _mesh.vertices to the _vertices
        _mesh.vertices = _vertices;
        //Set the _mesh.uvs to the mesh _uvs
        _mesh.uv = _uvs;
        //Set the  _mesh.triangles to the mesh  _triangles
        _mesh.triangles = _triangles;
        //Set the _mesh.normals to the mesh _normals
        _mesh.normals = _normals;
        //Set the Material to the meshTex
        meshRender.sharedMaterial.mainTexture = meshTex;
        //Call Recalculate Normals
        _mesh.RecalculateNormals();

        //Set the meshFilter mesh to _mesh
        meshFilter.mesh = _mesh;
    }
    
    //Function for using the noiseMap
    private void GenerateMeshNoise()
    {
        Mesh mesh = new Mesh();

        float topLX = (width - 1) * -0.5f;
        float topLZ = (height - 1) * 0.5f;

        triangles = new int[(width - 1) * (height - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        float currentHeight;

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                //must add topLX and subtract topLZ or the mesh will draw upside down (inside out)
                vertices[vertexIndex].Set(topLX + x, heightCurve.Evaluate(noiseMap[x, y]) * heightMultiplier, topLZ - y);

                uvs[vertexIndex].Set(x / (float)width, y / (float)height);

                //colors[vertexIndex] = Color.blue;

                currentHeight = noiseMap[x,y];

                for (int i = 0; i < regions.Length; ++i)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        meshTex.SetPixel(x, y, regions[i].colour);
                        break;
                    }
                }

                if (x < width - 1 && y < height - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + width + 1;
                    triangles[triangleIndex + 2] = vertexIndex + width;

                    triangleIndex += 3;

                    triangles[triangleIndex] = vertexIndex + width + 1;
                    triangles[triangleIndex + 1] = vertexIndex;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    triangleIndex += 3;
                }

                ++vertexIndex;
            }
        }
        //Apply to meshTex data to meshTex
        meshTex.Apply();

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.normals = normals;
        meshRender.sharedMaterial.mainTexture = meshTex;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void OnValidate()
    {
       // GenerateTerrain();
    }
}
