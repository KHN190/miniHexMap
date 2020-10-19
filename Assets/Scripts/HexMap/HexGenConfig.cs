using UnityEngine;

namespace MiniHexMap
{
    public enum NoiseType
    {
        Perlin, Simplex, Cellular
    }

    public class HexGenConfig : MonoBehaviour
    {
        [Header("Proc Gen")]
        public bool noText = true;
        public bool generateGrass = true;
        public bool generateTribe = true;
        [Range(0, 1)]
        public float grassDensity = .7f;
        [Range(0, 400)]
        public int tribeRadius = 200;
        [Range(0, 1)]
        public float tribeDensity = .3f;

        [Header("Noise Settings")]
        public NoiseType noiseType = NoiseType.Perlin;

        [Range(.1f, 5f)]
        public float noiseScale = 4f;
        [Range(1, 5)]
        public int noiseLayers = 3;
    }
}