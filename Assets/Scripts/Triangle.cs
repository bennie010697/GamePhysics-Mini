using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Triangle
{
    //World cordinate position 1 , 2 , 3 
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    //The normal to the triangle
    public Vector3 normal;
    //The area of the triangle
    public float area;
    //The center of the triangle
    public Vector3 center;
    //The distance to the surface from the center of the triangle
    public float distanceToSurface;

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.center = (p1 + p2 + p3) / 3f;
        this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;   //line p1->p2 , p1->p3 cross = normal.
        
        //Distance to the surface from the center of the triangle
        this.distanceToSurface = Mathf.Abs(WaterController.current.DistanceToWater(this.center));

        //area of triangle cal
        float a = Vector3.Distance(p1, p2);
        float b = Vector3.Distance(p1, p3);
        float c = Vector3.Distance(p2, p3);
        float s = (a + b + c) / 2;
        this.area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));   // https://www.mathsisfun.com/geometry/herons-formula.html
    }
}
