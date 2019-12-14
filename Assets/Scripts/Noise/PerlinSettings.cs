using UnityEngine;

namespace Noise
{
    [CreateAssetMenu(menuName="Noise/Perlin Settings", fileName="New Perlin Settings")]
    public class PerlinSettings : ScriptableObject
    {
        public NormalizeMode normalizeMode;

        public float scale;
        public int octaves;
        public float lacunarity;
        [Range(0f, 1f)] public float persistance;

        public int seed;
        public Vector2 offset;

        public void Validate()
        {
            scale = Mathf.Max(scale, 0.01f);
            octaves = Mathf.Max(octaves, 1);
            lacunarity = Mathf.Max(lacunarity, 1f);
            persistance = Mathf.Clamp01(persistance);
        }
    }
    
    public enum NormalizeMode
    {
        Local,
        Global
    }
}
