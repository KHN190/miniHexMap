using UnityEngine;

public class RandomNumberPool
{
    private int index;
    private const int size = 256 * 256;
    private float[] numbers;

    public RandomNumberPool(int seed = 0)
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

        float[] noises = new float[width * height];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                noises[i] = Mathf.PerlinNoise((float)x / width * scale + offset, (float)z / height * scale);

                i++;
            }
        }
        return noises;
    }
}
