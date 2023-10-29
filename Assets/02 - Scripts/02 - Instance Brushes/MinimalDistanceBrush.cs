using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimalDistanceBrush : InstanceBrush
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
            spawnObject(xRandom, zRandom);
        }
    }
}