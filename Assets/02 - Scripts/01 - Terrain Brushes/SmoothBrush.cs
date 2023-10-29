using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothBrush : TerrainBrush
{
    public int smoothingRadius = 3;
    public string smoothingType = "average";

    private float smoothAvgHeight(int x, int z, int smoothRadius)
    {
        float totalHeight = 0;
        int count = 0;
        for (int zi = -smoothRadius; zi <= smoothRadius; zi++)
        {
            for (int xi = -smoothRadius; xi <= smoothRadius; xi++)
            {
                totalHeight += terrain.get(x + xi, z + zi);
                count++;
            }
        }
        return totalHeight / count;
    }

    private float smoothGaussianHeight(int x, int z, int smoothRadius)
    {
        float totalHeight = 0;
        float totalWeight = 0;
        for (int zi = -smoothRadius; zi <= smoothRadius; zi++)
        {
            for (int xi = -smoothRadius; xi <= smoothRadius; xi++)
            {
                float distFromCenter = Mathf.Sqrt(zi * zi + xi * xi);
                float gaussianWeight = Mathf.Exp(-(distFromCenter * distFromCenter) / (2 * smoothRadius * smoothRadius));
                totalWeight += gaussianWeight;
                totalHeight += (terrain.get(x + xi, z + zi) * gaussianWeight);
            }
        }
        return totalHeight / totalWeight;
    }

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                if (smoothingType == "average")
                {
                    terrain.set(x + xi, z + zi, smoothAvgHeight(x + xi, z + zi, smoothingRadius));
                }
                if (smoothingType == "gaussian")
                {
                    terrain.set(x + xi, z + zi, smoothGaussianHeight(x + xi, z + zi, smoothingRadius));
                }
            }
        }
    }
}
