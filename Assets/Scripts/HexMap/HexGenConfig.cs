using UnityEngine;

public enum NoiseType
{
    Perlin, Simplex, Cellular
}

public class HexGenConfig : MonoBehaviour
{
    [Header("Proc Gen")]
    public bool generateGrass = true;
    [Range(0, 1)]
    public float grassDensity = .7f;

    [Header("Noise Settings")]
    public NoiseType noiseType = NoiseType.Perlin;

    [Range(.1f, 5f)]
    public float perlinScale = 4f;
    [Range(.1f, 5f)]
    public float simplexScale = 4f;
    [Range(1, 5)]
    public int noiseLayers = 3;
}
