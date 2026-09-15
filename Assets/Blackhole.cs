using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    public float bMass = 100f;
    public float bExp  = 2f;

    private bool gravCD;
    private int fixedUpdateCount = 0;

    void FixedUpdate()
    {
        fixedUpdateCount++;

        // Log every 50 fixed frames (~1 second) so we don't flood the console
        if (fixedUpdateCount % 50 == 0)
        {
            Debug.Log($"[BlackHole] FixedUpdate running. gravCD={gravCD}, " +
                      $"balls={(BouncyShoot.balls == null ? "NULL" : BouncyShoot.balls.Count.ToString())}, " +
                      $"gMult={Static.gMult}, gExp={Static.gExp}, massOverride={Static.massOverride}");
        }

        if (!gravCD)
            StartCoroutine(applyGrav(Static.bHIncrement));
    }

    public void blackHoleGravity()
    {
        if (BouncyShoot.balls == null)
        {
            Debug.LogWarning("[BlackHole] blackHoleGravity called but BouncyShoot.balls is null — skipping.");
            return;
        }

        if (BouncyShoot.balls.Count == 0)
        {
            // Not a problem, just no balls yet
            return;
        }

        float currentMass = Static.massOverride ? Static.gMult : bMass;
        float currentExp  = Static.expOverride  ? Static.gExp  : bExp;

        if (currentMass == 0)
        {
            // gMult is 0 so no force will be applied — this is expected when gravity is toggled off
            return;
        }

        float totalForceMagnitude = 0f;

        foreach (BallClass ballClass in BouncyShoot.balls)
        {
            if (ballClass.ball == null) continue;

            Vector3 direction = transform.position - ballClass.ball.transform.position;
            float dist = Mathf.Max(direction.magnitude, 0.1f);
            Vector3 force = direction.normalized * (currentMass / Mathf.Pow(dist, currentExp));

            ballClass.totalForceToAdd += force;
            totalForceMagnitude += force.magnitude;
        }

        // Log once per gravity tick when actually applying force
        Debug.Log($"[BlackHole] Applied gravity to {BouncyShoot.balls.Count} balls. " +
                  $"mass={currentMass}, exp={currentExp}, totalForceMag={totalForceMagnitude:F2}");
    }

    public IEnumerator applyGrav(float timeIncrement)
    {
        gravCD = true;
        blackHoleGravity();
        yield return new WaitForSeconds(timeIncrement);
        gravCD = false;
    }
}