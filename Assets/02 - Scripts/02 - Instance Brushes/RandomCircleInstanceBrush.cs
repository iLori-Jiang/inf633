using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomCircleInstanceBrush : InstanceBrush
{
    public override void draw(float x, float z)
    {
        float radiusRandom = Random.Range(0, radius);
        float angleRandom = Random.Range(0, 2 * Mathf.PI);
        spawnObject(x + radiusRandom * Mathf.Cos(angleRandom), z + radiusRandom * Mathf.Sin(angleRandom));
    }
}
