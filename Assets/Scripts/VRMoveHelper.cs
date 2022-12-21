using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRMoveHelper : MonoBehaviour
{
    private Unity.XR.CoreUtils.XROrigin XROrigin;
    private CharacterController cController;
    private CharacterControllerDriver driver;

    // Start is called before the first frame update
    void Start()
    {
        XROrigin = GetComponent<Unity.XR.CoreUtils.XROrigin>();
        cController = GetComponent<CharacterController>();
        driver = GetComponent<CharacterControllerDriver>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCharacterController();
    }

    protected virtual void UpdateCharacterController()
    {
        if (XROrigin == null || cController == null)
            return;

        var height = Mathf.Clamp(XROrigin.CameraInOriginSpaceHeight, driver.minHeight, driver.maxHeight);

        Vector3 center = XROrigin.CameraInOriginSpacePos;
        center.y = height / 2f + cController.skinWidth;

        cController.height = height;
        cController.center = center;
    }
}
