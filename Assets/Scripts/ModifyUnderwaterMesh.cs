using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyUnderwaterMesh
{
    private Transform objTrans;              //the object transform to get the world position of a vertice
    Vector3[] objVertices;                   //coordinates of the vertices in the object
    int[] objTriangles;                      //the indexes of the positions of the triangles    TODO: //Positions in allVerticesArray, such as 0, 3, 5, to build triangles ??????
    public Vector3[] objVerticesWorld;       //coordinates of the vertices in the world positions of the object //REMOVE? (or objVertices)
    float[] allDistancesToWater;             //list of all the distances to the water //REMOVE?
    public List<Triangle> underwaterTriangles = new List<Triangle>(); //all triangles that are under water
    public bool swap;                        //bool to check if swap in vertexdata has occured (to sort distance list)

    public ModifyUnderwaterMesh(GameObject obj)
    {
        objTrans = obj.transform;   

        //fill the arrays with the initial vertices / triangles of the object
        objVertices = obj.GetComponent<MeshFilter>().mesh.vertices;
        objTriangles = obj.GetComponent<MeshFilter>().mesh.triangles;
        objVerticesWorld = new Vector3[objVertices.Length];
        allDistancesToWater = new float[objVertices.Length];
    }

    public void MakeUnderwaterMesh()
    {
        underwaterTriangles.Clear();  //Clear Underwater triangle list (since loop and we don't want to add more and more meshes) //REMOVE? comment


        //safe the distance to water of the vertices (note that a triangle sometimes uses the same vertice)
        for (int i = 0; i < objVertices.Length; i++)
        {
            objVerticesWorld[i] = objTrans.TransformPoint(objVertices[i]);
            allDistancesToWater[i] = WaterController.currentWC.DistanceToWater(objTrans.TransformPoint(objVertices[i]));
        }

        //Create the triangles below the water so we can create a mesh.
        CreateTrianglesUnderwater();
    }

    //creating a vertexD object that can hold the data we need in a list. 
    private class VertexD
    {        
        public float distance;          //distance to water 
        public int index;               //index, needed to keep the correct triangles
        public Vector3 worldVertexPos;  
    }

    private void CreateTrianglesUnderwater()
    {
        List<VertexD> vertexData = new List<VertexD>(3); //TODO: check if (3) works

        int i = 0;
        //go through all the object triangles
        while (i < objTriangles.Length)
        {
            //go through the 3 points of the triangle
            for (int x = 0; x < 3; x++)
            {
                vertexData[x].distance = allDistancesToWater[objTriangles[i]];
                vertexData[x].index = x;
                vertexData[x].worldVertexPos = objVerticesWorld[objTriangles[i]];
                i++;
            }

            //--| Check where the vertices of the triangle are located (in water, outside water etc) |--
            
            //All vertices above fluid
            if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance > 0f)
            {
                continue;
            }

            //All vertices underwater
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
                //sort the distance of the vertices
                swap = false;
                for (int j = 0; j < 2; j++)
                {
                    OrderFunc(vertexData, j);
                    if (swap == true) //a swap has happened
                    {
                        j = 0;
                    }
                }

                //1 vertice above fluid, 2 under fluid
                if (vertexData[0].distance > 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                {
                    AddTrianglesPartiallyInWater(vertexData, 1);
                }
                //2 vertice above fluid, 1 under fluid
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
        //initialize the variables
        float hH = 0f;
        float hM = 0f;
        float hL = 0f;
        //initialize the vectors
        Vector3 H = Vector3.zero;
        Vector3 M = Vector3.zero;
        Vector3 L = Vector3.zero;

        // 1 vertice above water
        if (aboveVertices == 1)
        {
            //H is highest point (above water), M and L are still unknown
            H = vertexData[0].worldVertexPos;
            hH = vertexData[0].distance;

            //Since we need the triangle to be turned the same way we can't use the distance to surface for M and L but instead have to use the triangular index.
            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {   M_index = 2;   }                     
            
            //fill M and L according to M_index (if vertexdata[1] = M index it is M, else vertexdata[2] = M)
            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].worldVertexPos;
                L = vertexData[2].worldVertexPos;
                hM = vertexData[1].distance;
                hL = vertexData[2].distance;
            }
            else
            {
                M = vertexData[2].worldVertexPos;
                L = vertexData[1].worldVertexPos;
                hM = vertexData[2].distance;
                hL = vertexData[1].distance;
            }
            
            //calculate the triangular cutting points with formulas from: https://gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php

            //Point IM that cuts line H and M
            Vector3 MH = H - M;
            float tM = -hM / (hH - hM);
            Vector3 MI_M = tM * MH;
            Vector3 IM = MI_M + M;

            //Point IL that cuts line H and L
            Vector3 LH = H - L;
            float tL = -hL / (hH - hL);
            Vector3 LI_L = tL * LH;
            Vector3 IL = LI_L + L;

            //2 triangles below water added to the underwater list.
            underwaterTriangles.Add(new Triangle(M, IM, IL));
            underwaterTriangles.Add(new Triangle(M, IL, L));


        }
        // 2 vertices above water
        else
        {
            //L is lowest point, M and H are still unknown
            L = vertexData[2].worldVertexPos;
            hL = vertexData[2].distance;

            //Find correct index of H
            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }


            //fill H and M according to M_index
            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].worldVertexPos;
                M = vertexData[0].worldVertexPos;
                hH = vertexData[1].distance;
                hM = vertexData[0].distance;
            }
            else
            {
                H = vertexData[0].worldVertexPos;
                M = vertexData[1].worldVertexPos;
                hH = vertexData[0].distance;
                hM = vertexData[1].distance;
            }

            //calculate the triangular cutting points with formulas from: https://gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php

            //Point JM that cuts line L and M
            Vector3 LM = M - L;
            float tM = -hL / (hM - hL);
            Vector3 LJ_M = tM * LM;
            Vector3 JM = LJ_M + L;

            //Point JH that cuts line H and L
            Vector3 LH = H - L;
            float tH = -hL / (hH - hL);
            Vector3 LJ_H = tH * LH;
            Vector3 JH = LJ_H + L;

            //1 triangle below water added to the underwater list.
            underwaterTriangles.Add(new Triangle(L, JH, JM));
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
    public void DisplayMesh(Mesh mesh, string name, List<Triangle> trianglesP)  //REMOVE?
    {
        List<Vector3> vertices = new List<Vector3>(); //REMOVE?
        List<int> triangles = new List<int>();        //REMOVE?

        //Build the mesh
        for (int i = 0; i < trianglesP.Count; i++)
        {
            //From world to local coordinates
            Vector3 p1 = objTrans.InverseTransformPoint(trianglesP[i].p1);
            Vector3 p2 = objTrans.InverseTransformPoint(trianglesP[i].p2);
            Vector3 p3 = objTrans.InverseTransformPoint(trianglesP[i].p3);

            vertices.Add(p1);
            triangles.Add(vertices.Count - 1);
            vertices.Add(p2);
            triangles.Add(vertices.Count - 1);
            vertices.Add(p3);
            triangles.Add(vertices.Count - 1);
        }
               
        mesh.Clear(); //Remove old mesh                
        mesh.name = name; //Give it a name        
        mesh.vertices = vertices.ToArray(); //Add the new vertices and triangles
        mesh.triangles = triangles.ToArray(); //TODO check if naming correct?
        mesh.RecalculateBounds();
    }
}