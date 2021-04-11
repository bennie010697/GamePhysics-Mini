using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public static WaterController currentWC;

    void Start()
    {
        currentWC = this;
    }

    //0> above water surface, 0< below water surface
    public float DistanceToWater(Vector3 pos)
    {
        float distanceToWater = pos.y;
        return distanceToWater;
    }
}
