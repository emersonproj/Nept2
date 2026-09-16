using UnityEngine;
using UnityEngine.InputSystem;

// Updated for new Input System.
// Attach to the Main Camera GameObject.
// BouncyShoot.LateUpdate() decides when to call updateMouseOrbit / rotateLeft / rotateRight.
// This script contains no input handling of its own.

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class DragMouseOrbit : MonoBehaviour
{
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    // Called by BouncyShoot when right mouse + shift is held
    public void updateMouseOrbit(float distance, Vector3 target)
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        x += delta.x * xSpeed * (distance / 10)  * 0.002f;
        y -= delta.y * ySpeed * (distance / 50) * 0.002f;

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        transform.rotation = rotation;
        transform.position = rotation * new Vector3(0f, 0f, -distance) + target;
    }

    // Called by BouncyShoot when left arrow is held
    public void rotateLeft(float distance, Vector3 target)
    {
        x -= 0.1f;
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        transform.position = rotation * new Vector3(0f, 0f, -distance) + target;
        transform.LookAt(target);
    }

    // Called by BouncyShoot when right arrow is held
    public void rotateRight(float distance, Vector3 target)
    {
        x += 0.1f;
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        transform.position = rotation * new Vector3(0f, 0f, -distance) + target;
        transform.LookAt(target);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle >  360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}