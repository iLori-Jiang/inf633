using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSquareInstanceBrush : InstanceBrush
{

    public override void draw(float x, float z)
    {
            float xRandom = Random.Range(x - radius, x + radius);
            float zRandom = Random.Range(z - radius, z + radius);
            spawnObject(xRandom, zRandom);
    }
}