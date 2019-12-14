using UnityEngine;
using Random = System.Random;

namespace Noise
{
    public static class Perlin
    {
        public static float[] GenerateNoiseMap2D(int size, PerlinSettings settings, float sampleCentre)
        {
            settings.Validate();
            
            float[] noiseMap = new float[size];

            Random random = new Random(settings.seed);
            float[] octaveOffsets = new float[settings.octaves];

            float maxPossibleHeight = 0f;
            float amplitude = 1f;

            for (int i = 0; i < settings.octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + settings.offset.x + sampleCentre;
                octaveOffsets[i] = offsetX;

                maxPossibleHeight += amplitude;
                amplitude *= settings.persistance;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;
            float halfSize = size * 0.5f;

            for (int x = 0; x < size; x++)
            {
                amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfSize + octaveOffsets[i]) / settings.scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, 0) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                maxLocalNoiseHeight = Mathf.Max(maxLocalNoiseHeight, noiseHeight);
                minLocalNoiseHeight = Mathf.Min(minLocalNoiseHeight, noiseHeight);

                noiseMap[x] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x] + 1f) / (maxPossibleHeight / 0.9f);
                    noiseMap[x] = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue);
                }
            }

            if (settings.normalizeMode == NormalizeMode.Local)
            {
                for (int x = 0; x < size; x++)
                {
                    noiseMap[x] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x]);
                }
            }

            return noiseMap;
        }
        
        public static float[,] GenerateNoiseMap3D(int size, PerlinSettings settings, Vector2 sampleCentre)
        {
            settings.Validate();
            
            float[,] noiseMap = new float[size, size];

            Random random = new Random(settings.seed);
            Vector2[] octaveOffsets = new Vector2[settings.octaves];

            float maxPossibleHeight = 0f;
            float amplitude = 1f;

            for (int i = 0; i < settings.octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
                float offsetY = random.Next(-100000, 100000) + settings.offset.y + sampleCentre.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= settings.persistance;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;
            float halfSize = size * 0.5f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfSize + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfSize + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                maxLocalNoiseHeight = Mathf.Max(maxLocalNoiseHeight, noiseHeight);
                minLocalNoiseHeight = Mathf.Min(minLocalNoiseHeight, noiseHeight);

                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1f) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue);
                }
            }

            if (settings.normalizeMode == NormalizeMode.Local)
            {
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }
    }
}
