using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class GeometryLaunchManager : MonoBehaviour
{
    public static BouncyShoot bouncyShootRef;

    public static List<BallClass> fireCircle(GameObject modelParent, int ballCount, float size)
    {
        List<BallClass> ballStructs = new List<BallClass>();
        float step = 2 * Mathf.PI / ballCount;

        for (float theta = 0; theta < 2 * Mathf.PI; theta += step)
        {
            float x = 0 - size * Mathf.Cos(theta);
            float y = 0 - size * Mathf.Sin(theta);
            BallClass ballS = bouncyShootRef.ballFire(bouncyShootRef.gameObject,
                new Vector3(x, y, 0) * Static.ballSeperatness, Color.clear);
            ballS.ball.transform.parent = modelParent.transform;
            ballStructs.Add(ballS);
        }
        return ballStructs;
    }

    public List<BallClass> fireCircleCone(GameObject modelParent)
    {
        List<BallClass> ballStructs = new List<BallClass>();
        StartCoroutine(circleChainWait(modelParent, ballStructs));
        return ballStructs;
    }

    public IEnumerator circleChainWait(GameObject parent, List<BallClass> ballS)
    {
        for (int i = 1; i < 10; i++)
        {
            yield return new WaitForSecondsRealtime(.3f);
            ballS.AddRange(fireCircle(parent, i * 15, i));
        }
    }

    public List<BallClass> fireCircleDoubleCone(GameObject modelParent)
    {
        List<BallClass> ballStructs = new List<BallClass>();
        StartCoroutine(circleChainWait2(modelParent, ballStructs));
        return ballStructs;
    }

    public IEnumerator circleChainWait2(GameObject parent, List<BallClass> ballS)
    {
        // Temporarily switch to the alternate ball prefab if one is assigned
        GameObject bSaved = bouncyShootRef.BallInstYesPrefab;
        if (bouncyShootRef.ballPrefabs != null && bouncyShootRef.ballPrefabs.Count > 1)
            bouncyShootRef.BallInstYesPrefab = bouncyShootRef.ballPrefabs[1];

        for (int i = 1; i < 10; i++)
        {
            yield return new WaitForSecondsRealtime(.1f);
            ballS.AddRange(fireCircle(parent, i * 15, (float)i / 3));
        }
        for (int i = 9; i > 0; i--)
        {
            yield return new WaitForSecondsRealtime(.1f);
            ballS.AddRange(fireCircle(parent, i * 15, (float)i / 3));
        }

        bouncyShootRef.BallInstYesPrefab = bSaved;
    }
}