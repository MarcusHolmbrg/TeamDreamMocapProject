using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverMovement : MonoBehaviour
{
    public float moveSpeed;
    Vector3 observerPosition;
    public Transform orientation;

    // Start is called before the first frame update
    void Start()
    {
        observerPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            observerPosition += orientation.forward * moveSpeed / 50;
        }
        if (Input.GetKey(KeyCode.S))
        {
            observerPosition -= orientation.forward * moveSpeed / 50;
        }
        if (Input.GetKey(KeyCode.A))
        {
            observerPosition -= orientation.right * moveSpeed / 50;
        }
        if (Input.GetKey(KeyCode.D))
        {
            observerPosition += orientation.right * moveSpeed / 50;
        }

        this.transform.position = observerPosition;
    }
}
