using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightDependantBrush : InstanceBrush
{
    public int index = 1;
    public override void draw(float x, float z)
    {
        float xRandom = Random.Range(x - radius, x + radius);
        float zRandom = Random.Range(z - radius, z + radius);

        float scale_diff = Mathf.Abs(terrain.max_scale - terrain.min_scale);
        float scale_min = Mathf.Min(terrain.max_scale, terrain.min_scale);
        float scale = (float)CustomTerrain.rnd.NextDouble() * scale_diff + scale_min;

        terrain.spawnObject(terrain.getInterp3(x, z), scale, index);
    }
}


