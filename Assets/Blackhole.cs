using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

// Simple direct gravity — applies force to every ball every FixedUpdate.
// Tune bMass and bExp in the inspector.
//
// Suggested starting values:
//   bMass = 5000,  bExp = 1   → gentle linear pull, works at large distances
//   bMass = 5000,  bExp = 2   → inverse square (realistic gravity)
//   bMass = 50000, bExp = 2   → strong gravity

public class BlackHole : MonoBehaviour
{
    public float bExp  = 1f;     // falloff exponent (1=linear, 2=inverse square)

    void FixedUpdate()
    {
        if (BouncyShoot.balls == null || BouncyShoot.balls.Count == 0) return;

        foreach (BallClass ballClass in BouncyShoot.balls)
        {
            if (ballClass.ball == null) continue;

            Vector3 direction = transform.position - ballClass.ball.transform.position;
            float dist = Mathf.Max(direction.magnitude, 0.1f);

            ballClass.totalForceToAdd +=
                direction.normalized * (Static.gMult / Mathf.Pow(dist, Static.gExp));
        }
    }
}