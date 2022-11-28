using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("start");
        if(Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Pressing P");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse");
        };
    }
}
