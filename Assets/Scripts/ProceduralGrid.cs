using UnityEngine;
using System.Collections;
using System.IO;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    public int rows, columns;
    private Vector3[] _vertices;
    private Vector3[] _normals;


    public bool heightMap = true;
    public float minTerrainHeight = 0f;
    public float maxTerrainHeight = 10f;
    private Texture2D _tex;
    // Use this for initialization


    //PerlinNoiseGenerator perlin;

    int seed;

    public void GenerateMap()
    {
        _tex = LoadPNG("Assets/Resources/heightmap.png");

        int iTotalVertices = (rows + 1) * (columns + 1);
        _vertices = new Vector3[iTotalVertices];
        _normals = new Vector3[iTotalVertices];
        Generate();
    }

    private void Generate()
    {
        #region HeightMap
        if (heightMap)
        {
            MeshFilter _meshFilter = GetComponent<MeshFilter>();
            Mesh _mesh = new Mesh();
            _meshFilter.mesh = _mesh;

            float xRatio = _tex.width / rows;
            float yRatio = _tex.height / columns;

            Color[] _colours = new Color[(rows + 1) * (columns + 1)];

            int x = 0, z = 0;
            for (int i = 0; i < _vertices.Length; ++i)
            {
                Color pix = _tex.GetPixel((int)(x * xRatio), (int)(z * yRatio));
                float zVal = minTerrainHeight + maxTerrainHeight * pix.grayscale;

                _vertices[i].Set(x, zVal, z);
                ++x;
                if (x % (columns + 1) == 0)
                {
                    x = 0;
                    ++z;
                }

                if (zVal > (maxTerrainHeight / 2f))
                {
                    _colours[i].a = 1f;
                    _colours[i].r = 1f;
                    _colours[i].g = 1f;
                    _colours[i].b = 1f;
                }
                else if (zVal > (maxTerrainHeight / 3f))
                {
                    _colours[i].a = 1f;
                    _colours[i].r = 0f;
                    _colours[i].g = 1f;
                    _colours[i].b = 0f;
                }
                else
                {
                    _colours[i].a = 1f;
                    _colours[i].r = 0f;
                    _colours[i].g = 0f;
                    _colours[i].b = 1f;
                }
            }

            int numIndicies = columns * rows * 6;
            int[] triangles = new int[numIndicies];
            for (int ti = 0, vi = 0; vi < (columns * (rows + 1)); ++vi, ti += 6)
            {

                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + columns + 1;
                triangles[ti + 5] = vi + columns + 2;

                if (vi != 0 && (vi + 2) % (columns + 1) == 0)
                {
                    ++vi;
                }
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = triangles;
            _mesh.colors = _colours;
            _mesh.RecalculateNormals();
        }
        #endregion
        else
        {
            MeshFilter _meshFilter = GetComponent<MeshFilter>();
            Mesh _mesh = new Mesh();
            _meshFilter.mesh = _mesh;

            float[,] noiseMap = PerlinNoiseGenerator.GenerateNoise(rows, columns, 25.2f, 250);

            //int x = 0, z = 0;

            int vertexIndex = 0;
            for (int y = 0; y < columns; ++y)
            {
                for (int x = 0; x < rows; ++x)
                {
                    _vertices[vertexIndex].Set(x, noiseMap[y,x], y);

                    //if (x == 20)
                    //{
                    //    x = 0;
                    //    ++z;
                    //}
                    //++x;
                    ++vertexIndex;
                }
            }

            int numIndicies = columns * rows * 6;
            int[] triangles = new int[numIndicies];
            for (int ti = 0, vi = 0; vi < (columns * (rows + 1)); ++vi, ti += 6)
            {

                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + columns + 1;
                triangles[ti + 5] = vi + columns + 2;

                if (vi != 0 && (vi + 2) % (columns + 1) == 0)
                {
                    ++vi;
                }
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = triangles;
            //_mesh.colors = _colours;
            _mesh.RecalculateNormals();
        }
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
}
