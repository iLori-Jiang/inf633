using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacementBrush : InstanceBrush
{
    public float minimalDistance = 10f;
    public override void draw(float x, float z)
    {
        float xRandom = Random.Range(x - radius, x + radius);
        float zRandom = Random.Range(z - radius, z + radius);
        int count = terrain.getObjectCount();
        bool thereIsATree = false;
        for (int i = 0; i < count; i++)
        {
            Vector3 loc = terrain.getObjectLoc(i);
            float dist = Mathf.Sqrt((loc.x - xRandom) * (loc.x - xRandom) + (loc.z - zRandom) * (loc.z - zRandom));

            if (dist <= minimalDistance)
            {
                thereIsATree = true;
                break;
            } 
        }
        if (thereIsATree == false)
        {
            (int index, float scale_factor) = chooseTreeIndex(xRandom, zRandom);

            float scale_diff = Mathf.Abs(terrain.max_scale - terrain.min_scale);
            float scale_min = Mathf.Min(terrain.max_scale, terrain.min_scale);

            float scale = (float)(CustomTerrain.rnd.NextDouble() * scale_diff + scale_min) * scale_factor;
            if (index == -1 || scale_factor == 0) {
                return;
            }
            terrain.spawnObject(terrain.getInterp3(xRandom, zRandom), scale, index);
        }

    }

    // chooseTreeIndex determines which tree index and scale factor to use, depending on the current height and steepness
    private (int, float) chooseTreeIndex(float x, float z)
    {
        // index. 0: broad leaf tall tree, 1: small palm tree, 2: tall conifer, 3: tall broad leaf tree, 4/5/6: small bushes.
        float slope = terrain.getSteepness(x, z); 
        float height = terrain.getInterp(x, z); 
        int brushIndex = -1;
        float scale_factor = 0f;

        if (slope > 50) { 
            return (brushIndex, scale_factor);
        }
        // if height is larger than 15, plant a bush (index 4/5/6) or a conifer
        if (height > 20) 
        {
            brushIndex = (int)Random.Range(4, 8);
            scale_factor = (float)Random.Range(1, 3);
            if (height > 35) {
                scale_factor = (float)Random.Range(1, 2);
            }
            // plant a conifer 1/4 of the times
            if (brushIndex == 7) {
                brushIndex = 2;
                scale_factor = (float)Random.Range(0.20f, 0.60f);
            }
            return (brushIndex, scale_factor);
        } else if (height > 10) 
        {
            // plant confier, trees and bushes
            brushIndex = (int)Random.Range(2, 6);
            scale_factor = (float)Random.Range(0.30f, 0.80f); 
            if (brushIndex > 3) {
                scale_factor = (float)Random.Range(1.5f, 3.5f); 
            }
            return (brushIndex, scale_factor);
        } 
        // else, plant a broad leaf tree, for few remaining confiers/bushes
        brushIndex = (int)Random.Range(0, 4);
        scale_factor = (float)Random.Range(0.40f, 0.90f);
        if (brushIndex == 1) {
            brushIndex = 6;
            scale_factor = (float)Random.Range(1.5f, 3.5f);
        }
        return (brushIndex, scale_factor);
    }
}