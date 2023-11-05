using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianBrush : TerrainBrush
{

    public float height = 10.0f;

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                // gaussian coefficient
                float dist = new Vector2(xi, zi).magnitude;

                float exponent = -0.5f * Mathf.Pow(dist / (radius/3), 2);
                float coefficient = 1.0f / ((radius / 3) * Mathf.Sqrt(2 * Mathf.PI));
                float gaussian_noise = coefficient * Mathf.Exp(exponent);
                float terrainpoint = terrain.get(x + xi, z + zi);

                terrain.set(x + xi, z + zi, terrainpoint + height*gaussian_noise);
            }
        }
    }
}
