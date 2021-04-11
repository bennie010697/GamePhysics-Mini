using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Triangle
{
    //World cordinate position of triangle 1 , 2 , 3 
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public Vector3 normal;   //Normal of triangle
    public float area;       //Area of triangle
    public Vector3 center;   //Center of triangle
    public float distanceToSurface; //Distance to the water surface from the center of the triangle

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        
        this.center = (p1 + p2 + p3) / 3f;
        this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;   //line p1->p2 , p1->p3 cross = normal.
        this.distanceToSurface = Mathf.Abs(WaterController.currentWC.DistanceToWater(this.center));

        //calculation of area from: https://www.mathsisfun.com/geometry/herons-formula.html
        float a = Vector3.Distance(p1, p2);
        float b = Vector3.Distance(p1, p3);
        float c = Vector3.Distance(p2, p3);
        float s = (a + b + c) / 2;
        this.area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));   
    }
}
