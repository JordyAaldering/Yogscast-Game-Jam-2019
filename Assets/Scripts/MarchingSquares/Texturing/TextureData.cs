#pragma warning disable 0649
using UnityEngine;

namespace MarchingSquares.Texturing
{
    [CreateAssetMenu(menuName="Marching Squares/Texture Data", fileName="New Texture Data")]
    public class TextureData : ScriptableObject
    {
        [SerializeField] private Layer topLayer;
        [SerializeField, Range(0f, 1f)] private float topLayerSize = 0.5f;
        [SerializeField] private Layer[] layers;
        
        public Color[] GenerateColorMap(float[] heightMap, float[,] noiseMap, float startHeight, float totalHeight)
        {
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);
            
            Color[] colorMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float currentHeight = startHeight + y / totalHeight;
                if (currentHeight > heightMap[x] - topLayerSize)
                {
                    colorMap[x + y * width] = topLayer.color;
                    continue;
                }
                
                float noiseHeight = noiseMap[x, y];
                foreach (Layer layer in layers)
                {
                    if (noiseHeight <= layer.height)
                    {
                        colorMap[x + y * width] = layer.color;
                        break;
                    }
                }
            }

            return colorMap;
        }

        [System.Serializable]
        public class Layer
        {
            public string name = "Name";
            public Color color = Color.magenta;
            [Range(0f, 1f)] public float height = 1f;
        }
    }
}
