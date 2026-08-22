using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BouncyShoot : MonoBehaviour
{
    public static readonly Color colorZero = Color.clear;
    public static List<BallClass> balls;
    public static Dictionary<GameObject, List<BallClass>> parentObjects;

    public GameObject BallInstYesPrefab;
    public GameObject BallInstNoPrefab;
    public GameObject spherePrefab;

    public Vector3 mousePos;
    public Camera camera;

    private bool ballOnCool;

    // -------------------------------------------------------------------------
    // LIFECYCLE
    // -------------------------------------------------------------------------

    void Start()
    {
        balls = new List<BallClass>();
        BallInstYesPrefab = spherePrefab;
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        parentObjects = new Dictionary<GameObject, List<BallClass>>();
    }

    void Update()
    {
        // -- Aim at mouse --
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 1f));
        transform.LookAt(mouseWorldPos);

        // -- Fire (held) --
        if (Keyboard.current.fKey.isPressed)
        {
            if (!ballOnCool)
                StartCoroutine(ballCD(Static.ballCoolTime));
        }

        // -- Grid --
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            GameObject matrixParent = new GameObject("matrixParent");
            List<BallClass> MatrixList = new List<BallClass>();

            for (int i = 0; i < Static.matrixD1; i++)
            {
                for (int j = 0; j < Static.matrixD2; j++)
                {
                    for (int h = 0; h < Static.matrixD3; h++)
                    {
                        BallClass ballS = ballFire(this.gameObject, new Vector3(
                            i * Static.ballSize * Static.ballSeperatness - Static.ballSize * Static.ballSeperatness * ((Static.matrixD1 - 1) / 2f),
                            j * Static.ballSize * Static.ballSeperatness - Static.ballSize * Static.ballSeperatness * ((Static.matrixD2 - 1) / 2f),
                            h * Static.ballSize * Static.ballSeperatness), colorZero);
                        ballS.ball.transform.parent = matrixParent.transform;
                        MatrixList.Add(ballS);
                    }
                }
            }
            parentObjects[matrixParent] = MatrixList;
        }

        // -- Destroy all balls --
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            foreach (BallClass bs in balls)
            {
                foreach (List<BallClass> list in parentObjects.Values)
                {
                    if (list.Contains(bs))
                        list.Remove(bs);
                }
                Destroy(bs.ball);
            }
            balls.Clear();
        }

        // -- Spark away (tap K) --                         [STEP 1]
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            sparkAway();
        }
    }

    void FixedUpdate()
    {
        // -- Spark in (hold J) --                          [STEP 1]
        if (Keyboard.current.jKey.isPressed)
        {
            sparkIn();
        }

        addBallForces();
    }

    // -------------------------------------------------------------------------
    // BALL SPAWNING
    // -------------------------------------------------------------------------

    public BallClass ballFire(GameObject shootingObj, Vector3 addedVect, Color color)
    {
        GameObject newBall;
        if (color == Color.clear || color == colorZero || color == Color.black)
            newBall = Instantiate(BallInstYesPrefab, shootingObj.transform.position + addedVect, Quaternion.identity);
        else
            newBall = Instantiate(BallInstNoPrefab, shootingObj.transform.position + addedVect, Quaternion.identity);

        BallClass ballClass = new BallClass();
        ballClass.ball    = newBall;
        ballClass.bS      = newBall.GetComponent<BallScript>();
        ballClass.rB      = newBall.GetComponent<Rigidbody>();
        ballClass.relativeStartPos = addedVect;

        balls.Add(ballClass);

        if (color != colorZero && color != Color.black)
            newBall.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

        if (Static.velocityDirectToggle)
            ballClass.rB.linearVelocity = shootingObj.transform.forward * Static.ballVelocity;
        else
            ballClass.totalForceToAdd += shootingObj.transform.forward * Static.ballForce;

        newBall.transform.localScale = new Vector3(
            Static.ballSize * newBall.transform.localScale.x,
            Static.ballSize * newBall.transform.localScale.y,
            Static.ballSize * newBall.transform.localScale.z);

        return ballClass;
    }

    public IEnumerator ballCD(float cdTime)
    {
        ballOnCool = true;
        ballFire(this.gameObject, Vector3.zero, colorZero);
        yield return new WaitForSeconds(cdTime);
        ballOnCool = false;
    }

    // -------------------------------------------------------------------------
    // FORCES
    // -------------------------------------------------------------------------

    public void addBallForces()
    {
        foreach (BallClass ball in balls)
        {
            ball.rB.AddForce(ball.totalForceToAdd);
            ball.totalForceToAdd = Vector3.zero;
        }
    }

    // Tap K — blast all balls away from origin                [STEP 1]
    public void sparkAway()
    {
        foreach (BallClass bStruct in balls)
            bStruct.totalForceToAdd += bStruct.ball.transform.position.normalized * 20000;
    }

    // Hold J — pull all balls toward origin                   [STEP 1]
    public void sparkIn()
    {
        foreach (BallClass bStruct in balls)
            bStruct.totalForceToAdd += bStruct.ball.transform.position.normalized * -100;
    }

    // -------------------------------------------------------------------------
    // UTILITIES
    // -------------------------------------------------------------------------

    // Returns the average position of all live balls          [STEP 1]
    // (needed by returnToParentPosition and returnToAllPosition in later steps)
    public Vector3 calculateMidPoint(List<BallClass> ballsList)
    {
        Vector3 totalVect = Vector3.zero;
        for (int i = ballsList.Count - 1; i > -1; i--)
        {
            if (ballsList[i].ball != null)
                totalVect += ballsList[i].ball.transform.position;
            else
                ballsList.RemoveAt(i);
        }
        return totalVect / Mathf.Max(ballsList.Count, 1);
    }

    // Placeholder — materials wired up in a later step
    public void updateBallColor() { }
}