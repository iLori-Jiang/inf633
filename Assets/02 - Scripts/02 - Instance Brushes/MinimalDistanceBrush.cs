using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimalDistanceBrush : InstanceBrush
{
    public float minimalDistance = 10f;
    public int index = 1;
    public float scale_factor = 1;
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
            float scale_diff = Mathf.Abs(terrain.max_scale - terrain.min_scale);
            float scale_min = Mathf.Min(terrain.max_scale, terrain.min_scale);
            float scale = (float)(CustomTerrain.rnd.NextDouble() * scale_diff + scale_min) * scale_factor;

            terrain.spawnObject(terrain.getInterp3(xRandom, zRandom), scale, index);
            //spawnObject(xRandom, zRandom);
        }
    }
}