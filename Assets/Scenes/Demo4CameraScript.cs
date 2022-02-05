using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo4CameraScript : MonoBehaviour
{

    public GameObject target;

    private float radius;
    //private float ang;

    // Start is called before the first frame update
    void Start()
    {
        radius = Vector3.Distance(target.transform.position, transform.position);
        Debug.Log(radius);
        //ang = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //float xCurr = target.transform.position.x + radius * Mathf.Cos(ang * (float)(Math.PI / 180f)); // radians
        //float zCurr = target.transform.position.z + radius * Mathf.Sin(ang * (float)(Math.PI / 180f));
        //transform.position = new Vector3(xCurr, transform.position.y, zCurr);

        transform.RotateAround(target.transform.position, Vector3.up, -0.01f);
        transform.LookAt(target.transform);

        //ang += 0.05f;
    }
}
