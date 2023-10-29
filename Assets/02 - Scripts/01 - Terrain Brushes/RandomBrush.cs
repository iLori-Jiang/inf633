using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBrush : TerrainBrush
{
    public float strength = 0.1f;
    public float perlinFrequency = 1.0f;
    public float perlinSeed = 0.0f;

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                float distFromCenter = Mathf.Sqrt(zi * zi + xi * xi);
                float xcord = (x + xi) * perlinFrequency + perlinSeed;
                float zcord = (z + zi) * perlinFrequency + perlinSeed;
                float perlinValue = Mathf.PerlinNoise(xcord, zcord);
                terrain.set(x + xi, z + zi, terrain.get(x + xi, z + zi) + perlinValue * strength);
            }
        }
    }
}
