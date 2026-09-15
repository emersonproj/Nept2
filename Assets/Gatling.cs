using System.Collections;
using UnityEngine;

// Drop-in from original — no input system changes needed (BouncyShoot handles the keys).
// Add this as a component on the same GameObject as BouncyShoot.
// BouncyShoot.Start() sets bouncyShootRef automatically.

namespace DefaultNamespace
{
    public class Gatling : MonoBehaviour
    {
        public BouncyShoot bouncyShootRef;

        private bool firing;
        private bool circleFiring;
        private float circleTheta;

        public IEnumerator gatler(float startPos = 0, bool direction = true)
        {
            float ballNumberInCircle = 20;
            float sizeUpBy  = .03f;
            float originalSize = 4f;
            float currentSize  = originalSize;
            float theta = 2 * Mathf.PI * startPos;

            while (firing)
            {
                ballNumberInCircle = currentSize * 6.66f;
                float step = 2 * Mathf.PI / ballNumberInCircle;

                Vector3 vect = direction
                    ? new Vector3(0 - currentSize * Mathf.Cos(theta), 0 - currentSize * Mathf.Sin(theta), 0)
                    : new Vector3(0 + currentSize * Mathf.Cos(theta), 0 - currentSize * Mathf.Sin(theta), 0);

                bouncyShootRef.ballFire(bouncyShootRef.gameObject, vect, Color.black);
                theta       += step;
                currentSize += sizeUpBy;

                yield return new WaitForSecondsRealtime(0.01f);
            }
        }

        public IEnumerator gatlerCircle()
        {
            float circleStep           = 2 * Mathf.PI / 10;
            float individualCircleStep = 2 * Mathf.PI / 55;

            while (circleFiring)
            {
                Vector3 vect = new Vector3(0 - 2f * Mathf.Cos(circleTheta), 0 - 2f * Mathf.Sin(circleTheta), 0);

                for (float t = 0; t < 2 * Mathf.PI; t += individualCircleStep)
                {
                    float x = 0 - 1.2f * Mathf.Cos(t);
                    float y = 0 - 1.2f * Mathf.Sin(t);
                    bouncyShootRef.ballFire(bouncyShootRef.gameObject,
                        new Vector3(x + vect.x, y + vect.y, 0) * Static.ballSeperatness, Color.clear);
                }
                circleTheta += circleStep;

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        public void firingOn(float startPos = 0, bool direction = true)
        {
            firing = true;
            StartCoroutine(gatler(startPos, direction));
        }

        public void firingOff()
        {
            firing = false;
        }

        public void firingCircleToggle()
        {
            circleFiring = !circleFiring;
            if (circleFiring)
                StartCoroutine(gatlerCircle());
        }
    }
}