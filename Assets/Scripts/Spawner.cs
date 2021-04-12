using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Camera playerCamera;
    private float distance = 20.0f;
    public GameObject spawnee;
    public GameObject spawnee2;
    public Transform spawnPos;


    // Use this for initialization
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        spawnPos.position = playerCamera.transform.position + playerCamera.transform.forward * distance;
        spawnPos.rotation = new Quaternion(0.0f, playerCamera.transform.rotation.y, 0.0f, playerCamera.transform.rotation.w);
        //change mass per 1000 per click
        if (Input.GetKeyDown("u"))
        {
            spawnee.GetComponent<Rigidbody>().mass += 1000;
            spawnee2.GetComponent<Rigidbody>().mass += 1000;
        }
        if (Input.GetKeyDown("i"))
        {
            spawnee.GetComponent<Rigidbody>().mass -= 1000;
            spawnee2.GetComponent<Rigidbody>().mass -= 1000;
        }
        //hold to change mass fast.
        if (Input.GetKey("j"))
        {
            spawnee.GetComponent<Rigidbody>().mass += 100;
            spawnee2.GetComponent<Rigidbody>().mass += 100;
        }
        if (Input.GetKey("k"))
        {
            spawnee.GetComponent<Rigidbody>().mass -= 100;
            spawnee2.GetComponent<Rigidbody>().mass -= 100;
        }


        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(spawnee, spawnPos.position, spawnPos.rotation);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Instantiate(spawnee2, spawnPos.position, spawnPos.rotation);
        }
    }
}