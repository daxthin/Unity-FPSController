using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraLook
{
    // Private fields
    private static float m_mouseSensivity;
    private static float m_cameraPitch;
    private static float m_clampYRot;

    public static void SetupCamera(float mouseSensivity, float clampYRot)
    {
        m_mouseSensivity = mouseSensivity;
        m_clampYRot = clampYRot;
        if (clampYRot > 90)
        {
            m_clampYRot = 90;
        }
    }

    public static void MouseLook(Transform playerCamera, Transform playerTransform)
    {
        float h = Input.GetAxisRaw("Mouse X");
        float v = Input.GetAxisRaw("Mouse Y");

        m_cameraPitch -= v * m_mouseSensivity;
        playerCamera.localEulerAngles = Vector3.right * m_cameraPitch;
        playerTransform.Rotate(Vector3.up * h * m_mouseSensivity);
    }
}
