using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBehaviour : MonoBehaviour
{
    private Rigidbody objectRB;
    private ModifyUnderwaterMesh modifyUnderwaterMesh;

    private Vector3 V; //fluid volume
    private Vector3 buoyancy;

    //Densities of the fluid the object is falling in: (from: https://physics.info/density/)
    private float seaWaterDensity = 1025f;
    //private float honeyDensity    = 1420f;
    //private float cowMilkDensity  = 994f; //heavy cream
    //private float mercury         = 13594f;   


    void Start()
    {   //Load the game object and start the needed scripts
        objectRB = gameObject.GetComponent<Rigidbody>();
        modifyUnderwaterMesh = new ModifyUnderwaterMesh(gameObject);
    }

    void Update()
    {
        modifyUnderwaterMesh.CreateTrianglesUnderwater();
    }

    //Update per time step instead of frame (better for physics calculations since you dont want weird stuff based on frame rate)   
    void FixedUpdate()
    {
        List<Triangle> underwaterTriangles = modifyUnderwaterMesh.underwaterTriangles;

        //If object triangles below (or partially below) water, apply buoyancy
        if (underwaterTriangles.Count > 0)
        {
            for (int i = 0; i < underwaterTriangles.Count; i++)
            {
                //Calculate buoyancy
                Vector3 buoyancy = BuoyancyFunc(seaWaterDensity, underwaterTriangles[i]);

                //Add the buoyancy to the object
                objectRB.AddForceAtPosition(buoyancy, underwaterTriangles[i].center);
            }
        }
    }

    private Vector3 BuoyancyFunc(float density, Triangle triangle)
    {
        //Buoyancy = density*g*V 
        //V = fluid volume

        V = triangle.distanceToSurface * triangle.area * triangle.normal;
        buoyancy = density * Physics.gravity.y * V;

        //The x and z forces of the buoyancy cancel out, as we only care about the vertical force
        buoyancy.x = 0f;
        buoyancy.z = 0f;

        return buoyancy;
    }
}
