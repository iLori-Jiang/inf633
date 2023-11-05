using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterErosionBrush : TerrainBrush
{
    public float brushStrength = 0.1f;

    public override void draw(int x, int z)
    {
        float max_distance = Vector2.Distance(new Vector2(x + radius, z + radius), new Vector2(x, z));
        for (int xi = - radius; xi < radius; xi++)
        {
            for (int zi = - radius; zi < radius; zi++)
            {
                float distance = Vector2.Distance(new Vector2(x + xi, z + zi), new Vector2(x, z));
                float brushStrengthFactor = max_distance - distance;
                float terrainpoint = terrain.get(x + xi, z + zi);
                terrain.set(x + xi, z + zi, terrainpoint - brushStrength * brushStrengthFactor);
            }
        }
    }
}
