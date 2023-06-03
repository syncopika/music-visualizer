using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo4CameraScript : MonoBehaviour
{

    public GameObject target;

    private float radius;

    // Start is called before the first frame update
    void Start()
    {
        radius = Vector3.Distance(target.transform.position, transform.position);
        Debug.Log(radius);
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(target.transform.position, Vector3.up, -0.03f);
        transform.LookAt(target.transform);
    }
}