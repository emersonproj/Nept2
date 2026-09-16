using UnityEngine;
using UnityEngine.InputSystem;

// Simplified orbit using RotateAround — no accumulated angle state that can go stale.
// Attach to Main Camera. BouncyShoot.LateUpdate() calls these methods directly.

[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class DragMouseOrbit : MonoBehaviour
{
    public float xSpeed = 0.2f; // degrees of orbit per pixel of mouse movement
    public float ySpeed = 0.2f;
    public float arrowSpeed = 2f; // degrees per frame for arrow key orbit

    // Called by BouncyShoot when right mouse + shift is held
    public void updateMouseOrbit(Vector3 target)
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        if (delta == Vector2.zero) return;

        // Horizontal: orbit around world Y axis
        transform.RotateAround(target, Vector3.up, delta.x * xSpeed);

        // Vertical: orbit around camera's local right axis
        transform.RotateAround(target, transform.right, -delta.y * ySpeed);

        // Always keep looking at the target
        transform.LookAt(target);
    }

    // Called by BouncyShoot when left arrow is held
    public void rotateLeft(Vector3 target)
    {
        transform.RotateAround(target, Vector3.up, -arrowSpeed);
        transform.LookAt(target);
    }

    // Called by BouncyShoot when right arrow is held
    public void rotateRight(Vector3 target)
    {
        transform.RotateAround(target, Vector3.up, arrowSpeed);
        transform.LookAt(target);
    }
}