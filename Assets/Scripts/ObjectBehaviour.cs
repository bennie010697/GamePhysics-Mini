using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBehaviour : MonoBehaviour
{
    //drags
    public GameObject underWaterObj;
    private Rigidbody objectRB;      //The objects rigidbody
    private Mesh underWaterMesh; //Mesh for debugging //REMOVE?

    private ModifyUnderwaterMesh modifyUnderwaterMesh; //calling script that finds underwater triangles


    //Densities of the fluid the object is falling in: (from: https://physics.info/density/)
    private float seaWaterDensity = 1025f;
    //private float honeyDensity    = 1420f;
    //private float cowMilkDensity  = 994f; //heavy cream
    //private float mercury         = 13594f;   
        

    void Start()
    {
        //Get the obj's rigidbody thanks to unity //REMOVE? comment
        objectRB = gameObject.GetComponent<Rigidbody>();
        modifyUnderwaterMesh = new ModifyUnderwaterMesh(gameObject); //the script will modify the obj mesh
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh; //the mesh for underwater //REMOVE?
    }

    void Update()
    {
        modifyUnderwaterMesh.MakeUnderwaterMesh();
        //Show the underwater mesh //REMOVE? (I feel like this doesn't work?)
        modifyUnderwaterMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh", modifyUnderwaterMesh.underwaterTriangles);
    }

    //Update per time step instead of frame (better for physics calculations since you dont want weird stuff based on frame rate)   
    //TODO: IN ifstatement non CtrlC + CtrlV like maken.
    void FixedUpdate()
    {
        List<Triangle> underwaterTriangles = modifyUnderwaterMesh.underwaterTriangles;

        //If object below (or partially below) water, apply buoyancy
        if (underwaterTriangles.Count > 0)
        {           
            for (int i = 0; i < underwaterTriangles.Count; i++)
            {
                Triangle triangle = underwaterTriangles[i];
                //Calculate buoyancy
                Vector3 buoyancy = BuoyancyFunc(seaWaterDensity, triangle);

                //Add the buoyancy to the object
                objectRB.AddForceAtPosition(buoyancy, triangle.center);

                //REMOVE? Below? 
                //Debug                
                Debug.DrawRay(triangle.center, triangle.normal * 3f, Color.white); //Normal                
                Debug.DrawRay(triangle.center, buoyancy.normalized * -3f, Color.blue); //Buoyancy
            }
        }
    }


    private Vector3 BuoyancyFunc(float density, Triangle triangle)
    {
        //Buoyancy = density*g*V 
        //V = fluid volume = TODO (can't find the formula)

        Vector3 V = triangle.distanceToSurface * triangle.area * triangle.normal;
        Vector3 buoyancy = density * Physics.gravity.y * V;

        //The x and z forces of the buoyancy cancel out, as we only care about the vertical force
        buoyancy.x = 0f;
        buoyancy.z = 0f;

        return buoyancy;
    }
}
