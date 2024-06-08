using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }


    void Update()
    {
        transform.LookAt(transform.position + (transform.position - cam.transform.position));
    }
}
