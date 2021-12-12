using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// can display pictures using this along with music visualization
public class Cubes : MonoBehaviour
{
    public GameObject cube;
    public Material mat;
    public int numObjects;
    public int xRange;
    public int yRange;
    public int zRange;
    public Vector3 scale;
    
    private List<GameObject> cubes;
    
    void rotateAll(){
        foreach(GameObject obj in cubes){
            obj.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
        }
    }
    
    void Start()
    {
        System.Random rnd = new System.Random();
        cubes = new List<GameObject>();
        for(int i = 0; i < numObjects; i++){
            // place new cube in random position and rotation
            int randX = rnd.Next((int)-xRange / 2, (int)xRange / 2);
            int randY = rnd.Next((int)-yRange / 2, (int)yRange / 2);
            int randZ = rnd.Next(-10, (int)zRange - 10);
            
            GameObject newObj = Instantiate(cube, new Vector3(randX, randY, randZ), UnityEngine.Random.rotation);
            newObj.transform.localScale = scale;
            newObj.GetComponent<Renderer>().material = mat;
            cubes.Add(newObj);
        }
    }

    void Update()
    {
        rotateAll();
    }
}