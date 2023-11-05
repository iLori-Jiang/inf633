using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBrush : InstanceBrush
{
    public int step = 1;
    public int order = 1;
    public float density = 1f;
    public float heightLowerLimit = 0f;
    public float heightUpperLimit = 15f;

    public override void draw(float x, float z)
    {
        if (step < 1)
        {
            step = 1;
        }

        if (order < 1)
        {
            order = 1;
        }else if (order > 2)
        {
            order = 2;
        }

        if (density < 0)
        {
            density = 0;
        }else if (density > 1)
        {
            density = 1;
        }

        for (int zi = -radius; zi <= radius; zi += step)
        {
            for (int xi = -radius; xi <= radius; xi += step)
            {
                // generate probability
                float dist = Mathf.Sqrt(Mathf.Pow(xi, 2) + Mathf.Pow(zi, 2)) / radius;
                float alpha = 1f;
                if (order == 1)
                {
                    alpha = -1 * dist + 1;
                }else if (order == 2)
                {
                    alpha = -1 * Mathf.Pow(dist, 2) + 1;
                }
                

                // random seed on this position
                float randomValue = Random.Range(0f, 1f);
                if (randomValue <= density * alpha)
                {
                    // if reach height limit
                    if ((heightLowerLimit <= terrain.getInterp(x + xi, z + zi)) && (terrain.getInterp(x + xi, z + zi) <= heightUpperLimit))
                    {
                        // if reach slope limit
                        if (terrain.getSteepness(x + xi, z + zi) < 35)
                        {
                            // generate object
                            spawnObject(x + xi, z + zi);
                        }
                    }
                }
            }
        }
    }
}
