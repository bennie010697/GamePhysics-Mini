using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBehaviour : MonoBehaviour
{
    //drags
    public GameObject underWaterObj;

    //The density of the water the boat is traveling in
    private float SeaWaterDensity = 1027f;

    //The objects rigidbody
    private Rigidbody ObjectRB;

    //Mesh for debugging
    private Mesh underWaterMesh;

    //Script that's doing everything needed with the boat mesh, such as finding out which part is above the water
    private ModifyUnderwaterMesh modifyUnderwaterMesh;

    // Start is called before the first frame update
    void Start()
    {
        //Get the obj's rigidbody thanks to unity.
        ObjectRB = gameObject.GetComponent<Rigidbody>();

        //Init the script that will modify the obj mesh
        modifyUnderwaterMesh = new ModifyUnderwaterMesh(gameObject);
        
        //Meshes that are below and above the water
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;

    }

    // Update is called once per frame
    void Update()
    {
        //Make the under water mesh
        modifyUnderwaterMesh.MakeUnderwaterMesh();
        // Display the under water mesh
        modifyUnderwaterMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh", modifyUnderwaterMesh.underwaterTriangles);
    }

    //Update per time step instead of frame (better for physics calculations since you dont want weird stuff based on frame rate)   TODO: IN ifstatement non CtrlC + CtrlV like maken.
    void FixedUpdate()
    {
        if (modifyUnderwaterMesh.underwaterTriangles.Count > 0)
        {
            //Get all triangles
            List<Triangle> underwaterTriangles = modifyUnderwaterMesh.underwaterTriangles;

            for (int i = 0; i < underwaterTriangles.Count; i++)
            {
                //This triangle
                Triangle triangle = underwaterTriangles[i];

                //Calculate the buoyancy force
                Vector3 buoyancyForce = BuoyancyForce(SeaWaterDensity, triangle);

                //Add the force to the boat
                ObjectRB.AddForceAtPosition(buoyancyForce, triangle.center);


                //Debug

                //Normal
                Debug.DrawRay(triangle.center, triangle.normal * 3f, Color.white);

                //Buoyancy
                Debug.DrawRay(triangle.center, buoyancyForce.normalized * -3f, Color.blue);
            }
        }
    }


    private Vector3 BuoyancyForce(float rho, Triangle triangle)
    {
        //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

        // F_buoyancy = rho * g * V
        Vector3 buoyancyForce = rho * Physics.gravity.y * (triangle.distanceToSurface * triangle.area * triangle.normal);

        //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        return buoyancyForce;
    }
}
