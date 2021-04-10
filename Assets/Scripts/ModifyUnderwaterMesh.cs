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
    // checks if swap in vertexdata has occured.
    public bool swap;

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
            else
            {
                //sort the vertexData biggest in front
                swap = false;
                for (int j = 0; j < 2; j++)
                {
                    OrderFunc(vertexData, j);
                    if (swap == true)
                    {
                        j = 0;
                    }
                }

                //One vertice is above the water (above 0 distance), the rest is below
                if (vertexData[0].distance > 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                {
                    AddTrianglesPartiallyInWater(vertexData, 1);
                }
                //Two vertices are above the water (above 0 distance), the other is below
                else
                {
                    AddTrianglesPartiallyInWater(vertexData, 2);
                }
            }
        }
    }

    /*AddTrianglesPartiallyInWater handles the triangles with formulas from the website: https://gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php
      Based on if the triangle has 1 or 2 vertices above water we will have to change the formula.*/
    private void AddTrianglesPartiallyInWater(List<VertexD> vertexData, int aboveVertices)
    {
        //initialize the variable
        float h_H = 0f;
        float h_M = 0f;
        float h_L = 0f;
        //initialize the vectors
        Vector3 H = Vector3.zero;
        Vector3 M = Vector3.zero;
        Vector3 L = Vector3.zero;

        // We have 1 vertice above water
        if (aboveVertices == 1)
        {
            //H is highest point and M and L are zero vectors since unknown still.
            H = vertexData[0].worldVertexPos;
            //Since we need the triangle to be turned the same way we can't use the distance to surface for M and L but instead have to use the triangular index.
            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {
                M_index = 2;
            }
            //We also need the heights to water H is known M and L still unknown since indexing.
            h_H = vertexData[0].distance;


            //fill M and L according to M_index (if vertexdata[1] has M index it is M.) else vertexdata[1] = L
            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].worldVertexPos;
                L = vertexData[2].worldVertexPos;
                h_M = vertexData[1].distance;
                h_L = vertexData[2].distance;
            }
            //reverse
            else
            {
                M = vertexData[2].worldVertexPos;
                L = vertexData[1].worldVertexPos;
                h_M = vertexData[2].distance;
                h_L = vertexData[1].distance;
            }
            //calculate the triangular cutting points with formulas from: https://gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php

            //Point I_M
            Vector3 MH = H - M;
            float t_M = -h_M / (h_H - h_M);
            Vector3 MI_M = t_M * MH;
            Vector3 I_M = MI_M + M;

            //Point I_L
            Vector3 LH = H - L;
            float t_L = -h_L / (h_H - h_L);
            Vector3 LI_L = t_L * LH;
            Vector3 I_L = LI_L + L;

            //2 triangles below the water added to the underwater list.
            underwaterTriangles.Add(new Triangle(M, I_M, I_L));
            underwaterTriangles.Add(new Triangle(M, I_L, L));


        }
        // we have 2 vertices above water
        else
        {
            //L is lowest point
            L = vertexData[2].worldVertexPos;
            //Find the index of H
            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }
            //We also need the depth in water of H_L
            h_L = vertexData[2].distance;

            //fill H and M according to H_index (if vertexdata[1] has H index it is H.) else vertexdata[1] = M
            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].worldVertexPos;
                M = vertexData[0].worldVertexPos;
                h_H = vertexData[1].distance;
                h_M = vertexData[0].distance;
            }
            //reverse
            else
            {
                H = vertexData[0].worldVertexPos;
                M = vertexData[1].worldVertexPos;
                h_H = vertexData[0].distance;
                h_M = vertexData[1].distance;
            }
            //calculate the triangular cutting points with formulas from: https://gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php

            //Point J_M
            Vector3 LM = M - L;
            float t_M = -h_L / (h_M - h_L);
            Vector3 LJ_M = t_M * LM;
            Vector3 J_M = LJ_M + L;

            //Point J_H
            Vector3 LH = H - L;
            float t_H = -h_L / (h_H - h_L);
            Vector3 LJ_H = t_H * LH;
            Vector3 J_H = LJ_H + L;

            //1 triangle below the water added to the underwater list.
            underwaterTriangles.Add(new Triangle(L, J_H, J_M));
        }
    }


    //swap function
    private void OrderFunc (List<VertexD> vertexData,int i)
    {
        float d1 = vertexData[i].distance;
        float d2 = vertexData[i + 1].distance;
        swap = false;
        if (d2 > d1)
        {
            VertexD help = vertexData[i];
            vertexData[i] = vertexData[i + 1];
            vertexData[i + 1] = help;
            swap = true;
        }
    }

    //TODO: EDIT dit NIET CTRL C + CTRL V
    //Display the underwater mesh
    public void DisplayMesh(Mesh mesh, string name, List<Triangle> trianglesList)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Build the mesh
        for (int i = 0; i < trianglesList.Count; i++)
        {
            //From global coordinates to local coordinates
            Vector3 p1 = objTrans.InverseTransformPoint(trianglesList[i].p1);
            Vector3 p2 = objTrans.InverseTransformPoint(trianglesList[i].p2);
            Vector3 p3 = objTrans.InverseTransformPoint(trianglesList[i].p3);

            vertices.Add(p1);
            triangles.Add(vertices.Count - 1);
            vertices.Add(p2);
            triangles.Add(vertices.Count - 1);
            vertices.Add(p3);
            triangles.Add(vertices.Count - 1);
        }

        //Remove the old mesh
        mesh.Clear();

        //Give it a name
        mesh.name = name;

        //Add the new vertices and triangles
        mesh.vertices = vertices.ToArray();

        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
    }
}