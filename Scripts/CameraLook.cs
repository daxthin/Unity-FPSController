using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraLook
{
    // Private fields
    private static float _mouseSensivity;
    private static float _cameraPitch;
    private static float _clampYRot;

    public static void SetupCamera(float mouseSensivity,float clampYRot)
    {
        _mouseSensivity = mouseSensivity;
        _clampYRot = clampYRot;
        if (clampYRot > 90)
        {
            _clampYRot = 90;
        }
    }

    public static void MouseLook(Transform playerCamera, Transform playerTransform)
    {
        float _h = Input.GetAxisRaw("Mouse X");
        float _v = Input.GetAxisRaw("Mouse Y");

        _cameraPitch -= _v * _mouseSensivity;

        playerCamera.localEulerAngles = Vector3.right * _cameraPitch;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -_clampYRot, _clampYRot);

        playerTransform.Rotate(Vector3.up * _h * _mouseSensivity);
    }
}
