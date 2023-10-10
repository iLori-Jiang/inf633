using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinBrush : TerrainBrush
{

    public float height = 5f;
    private float factor = 3;
    public float perlinScale = 10f;

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                float temp_height = terrain.get(x + xi, z + zi);
                float dist = Mathf.Sqrt(Mathf.Pow(xi, 2) + Mathf.Pow(zi, 2));
                // gaussian coeff
                float param = dist / (radius / factor);
                float alpha = Mathf.Exp(-Mathf.Pow(param, 2) / 2) / ((radius / factor) * Mathf.Sqrt(2 * Mathf.PI));

                // perlin coeff
                float perlin_1 = Mathf.PerlinNoise((x + xi) / perlinScale, (z + zi) / perlinScale);
                float perlin_2 = Mathf.PerlinNoise((x + xi) / perlinScale / 2, (z + zi) / perlinScale * 2);

                terrain.set(x + xi, z + zi, temp_height + alpha * (1 + perlin_1 - perlin_2) * height);
            }
        }
    }
}
