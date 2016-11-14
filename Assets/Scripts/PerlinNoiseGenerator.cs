using UnityEngine;
using System.Collections;

public static class PerlinNoiseGenerator
{
    public static float[,] GenerateNoise(int width, int height, float scale, int seed)
    {
        float[,] noiseMap = new float[width, height];

        if (scale <= 0)
        {
            //change scale to 0.0001f, this is because if scale is 0 nothing will happen
            scale = 0.0001f;
        }

       // float halfWidth = width * 0.5f;
        //float halfHeight = height * 0.5f;

        for(int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                float perlin = Mathf.PerlinNoise((x + seed) / scale, (y + seed) / scale);

                noiseMap[x,y] = perlin;
            }
        }
        return noiseMap;
    }
}
