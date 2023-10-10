using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearBrush : TerrainBrush
{

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                if (Mathf.Pow(xi, 2) + Mathf.Pow(zi, 2) <= Mathf.Pow(radius, 2))
                {
                    terrain.set(x + xi, z + zi, 0);
                }
            }
        }
    }
}
