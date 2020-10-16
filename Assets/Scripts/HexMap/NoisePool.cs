using UnityEngine;

namespace MiniHexMap
{
    public class NoisePool
    {
        private int index;
        private const int size = 256 * 256;
        private float[] numbers;

        public NoisePool(int seed = 0)
        {
            index = 0;
            numbers = new float[size];

            Random.InitState(seed);

            for (int i = 0; i < size; i++)
            {
                numbers[i] = Random.value;
            }
        }

        public float Next()
        {
            float f = numbers[index];
            index = (index + 1) % size;
            return f;
        }

        public static float[] Perlin(int width, int height, float scale = 1, float offset = 0, int seed = 0)
        {
            Random.InitState(seed);

            return GenNoise(Mathf.PerlinNoise, width, height, scale, offset);
        }

        public static float[] Simplex(int width, int height, float scale = 1, float offset = 0, int seed = 0)
        {
            Random.InitState(seed);

            FastNoiseLite noise = new FastNoiseLite();

            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            float[] noises = GenNoise(noise.GetNoise, width, height, scale * 50f, offset * 50f);

            for (int i = 0; i < noises.Length; i++)
            {
                // noises[i] = Mathf.Clamp01(noises[i] * -.85f + .15f);
                noises[i] = Mathf.Clamp01(noises[i] * .4f + .5f);
            }
            return noises;
        }

        public static float[] Cellular(int width, int height, float scale = 1, float offset = 0, int seed = 0)
        {
            Random.InitState(seed);

            FastNoiseLite noise = new FastNoiseLite();

            noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);

            float[] noises = GenNoise(noise.GetNoise, width, height, scale * 100f, offset * 100f);

            for (int i = 0; i < noises.Length; i++)
            {
                noises[i] = noises[i] * -.85f - .15f;
            }
            return noises;
        }

        private delegate float NoiseFunction(float x, float y);

        private static float[] GenNoise(NoiseFunction f, int width, int height, float scale = 1, float offset = 0)
        {
            float[] noises = new float[width * height];

            for (int z = 0, i = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    noises[i] = f((float)x / width * scale + offset, (float)z / height * scale);

                    i++;
                }
            }
            return noises;
        }

        public static float NoiseLayerInitialWeight(int layers)
        {
            float n = 0;
            for (int i = 0; i < layers; i++)
            {
                n += Mathf.Pow(.25f, i);
            }
            return 1 / n;
        }
    }
}