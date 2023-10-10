using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RaiseBrush : TerrainBrush
{

    public float height = 5;
    private float factor = 3;

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
                float alpha = Mathf.Exp(- Mathf.Pow(param, 2) / 2) / ((radius / factor) * Mathf.Sqrt(2 * Mathf.PI));

                terrain.set(x + xi, z + zi, temp_height + alpha * height);
            }
        }
    }
}
