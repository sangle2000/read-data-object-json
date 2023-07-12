using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject tree;

    public GameObject cubePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity);
            cube.transform.SetParent(tree.transform, false);
        }
    }
}
