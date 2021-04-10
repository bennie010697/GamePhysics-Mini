using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyUnderwaterMesh
{
    //object position
    private Transform objTrans;
    //Coordinates of all vertices in the original object
    Vector3[] objVertices;
    //Positions in allVerticesArray, such as 0, 3, 5, to build triangles ??????
    int[] objTriangles;

    //list of world positions off the object.
    public Vector3[] objVerticesWorld;
    //Find all the distances to water
    float[] allDistancesToWater;
    //all triangles from underwater mesh
    public List<Triangle> underwaterTriangles = new List<Triangle>();

    public ModifyUnderwaterMesh(GameObject obj)
    {
        //Get the transform off the object
        objTrans = obj.transform;

        //fill the arrays with the initial vertices / triangles of the object
        objVertices = obj.GetComponent<MeshFilter>().mesh.vertices;
        objTriangles = obj.GetComponent<MeshFilter>().mesh.triangles;

        //The boat vertices in global position
        objVerticesWorld = new Vector3[objVertices.Length];
        //Find all the distances to water once because some triangles share vertices, so reuse
        allDistancesToWater = new float[objVertices.Length];
    }
    //make the underwater mash
    public void MakeUnderwaterMesh()
    {
        //Clear Underwater triangle list (since loop and we don't want to add more and more meshes)
        underwaterTriangles.Clear();

        for (int i = 0; i < objVertices.Length; i++)
        {
            //get the world pos off the objects
            Vector3 worldPos = objTrans.TransformPoint(objVertices[i]);
            //safe the world positions off the vertices
            objVerticesWorld[i] = worldPos;
            //safe the distance to water off the vertices (note that a triangle sometimes uses the same vertice)
            allDistancesToWater[i] = WaterController.current.DistanceToWater(worldPos);
        }
        //Make the triangles below the water so we can create a mesh.
        CreateTrianglesUnderwater();
    }
    //creating a vertexD object that can hold the data we need in a list. 
    private class VertexD
    {
        //The distance to water from this vertex
        public float distance;
        //An index so we can form clockwise triangles
        public int index;
        //The global Vector3 position of the vertex
        public Vector3 worldVertexPos;
    }

    private void CreateTrianglesUnderwater()
    {
        List<VertexD> vertexData = new List<VertexD>(3);


        int i = 0;
        while (i < objTriangles.Length)
        {
            //Loop through the 3 vertices
            for (int x = 0; x < 3; x++)
            {
                //Save the data we need
                vertexData[x].distance = allDistancesToWater[objTriangles[i]];
                vertexData[x].index = x;
                vertexData[x].worldVertexPos = objVerticesWorld[objTriangles[i]];
                i++;
            }

            //All vertices are above the water
            if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance > 0f)
            {
                continue;
            }
            //add the triangles below water

            //All vertices are underwater
            if (vertexData[0].distance < 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
            {
                Vector3 p1 = vertexData[0].worldVertexPos;
                Vector3 p2 = vertexData[1].worldVertexPos;
                Vector3 p3 = vertexData[2].worldVertexPos;

                //Save the triangle
                underwaterTriangles.Add(new Triangle(p1, p2, p3));
            }
        }

    }
}