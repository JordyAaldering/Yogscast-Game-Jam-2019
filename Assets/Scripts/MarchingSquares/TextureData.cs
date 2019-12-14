using System;
using UnityEngine;

namespace Noise
{
    [CreateAssetMenu(menuName="Marching Squares/Texture Data", fileName="New Texture Data")]
    public class TextureData : ScriptableObject
    {
        public Layer[] layers;
        
        public Color[] GenerateColorMap(float[,] noiseMap)
        {
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);
            
            Color[] colorMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float currentHeight = noiseMap[x, y];
                foreach (Layer layer in layers)
                {
                    if (currentHeight <= layer.height)
                    {
                        colorMap[y * width + x] = layer.color;
                        break;
                    }
                }
            }

            return colorMap;
        }

        [Serializable]
        public class Layer
        {
            public Color color;
            [Range(0f, 1f)] public float height = 0f;
        }
    }
}
