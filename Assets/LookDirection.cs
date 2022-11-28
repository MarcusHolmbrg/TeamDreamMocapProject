using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookDirection : MonoBehaviour
{
    public float xSens;
    public float ySens;
    float xRot;
    float yRot;

    public Transform lookDir;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
      // mouse input
      float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSens;
      float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySens;

      xRot -= mouseY;
      yRot += mouseX;

      // limits PoV up/down
      xRot = Mathf.Clamp(xRot, -90f, 90f);

      //transform.rotation = Quaternion.Euler(xRot, yRot, 0);

      // rotate cam
      lookDir.rotation = Quaternion.Euler(xRot, yRot, 0);
    }
}
