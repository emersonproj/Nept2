using UnityEngine;
using UnityEngine.InputSystem;

// Updated for new Input System.
// Attach to the Main Camera GameObject.
// BouncyShoot.LateUpdate() decides when to call updateMouseLook.
// This script contains no input handling of its own.

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 50.0f;

    private float rotY = 0.0f;
    private float rotX = 0.0f;

    void Start()
    {
        setRotationToCurrent();
    }

    // Called by BouncyShoot when right mouse is held (without shift)
    public void updateMouseLook()
    {
        // Guard against division by zero when the game is paused (timeStep = 0)
        float timeStep = Mathf.Max(Static.timeStep, 0.01f);
        mouseSensitivity = 80f / timeStep;

        Vector2 delta = Mouse.current.delta.ReadValue();
        rotY +=  delta.x * mouseSensitivity * 0.001f;
        rotX += -delta.y * mouseSensitivity * 0.001f;

        transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    // Call this whenever the camera snaps to a new orientation so look
    // direction stays consistent across mode switches
    public void setRotationToCurrent()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }
}