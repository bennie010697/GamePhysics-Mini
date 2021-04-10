using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controlls the water
public class WaterController : MonoBehaviour
{
    public static WaterController current;

    void Start()
    {
        current = this;
    }

    //Positive if above water, Negative if below 
    public float DistanceToWater(Vector3 pos)
    {
        float distanceToWater = pos.y;

        return distanceToWater;
    }
}
