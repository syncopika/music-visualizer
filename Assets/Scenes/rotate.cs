using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{

    public float x = 0;
    public float y = 1;
    public float z = 0;
    public float speed = 8;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.position, new Vector3(x, y, z), Time.deltaTime * speed);
    }
}
