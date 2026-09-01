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
    public float spaceMult  = 0.008f;
    public float spaceAccel = 0.4f;

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
            if (!ballOnCool)
                StartCoroutine(ballCD(Static.ballCoolTime));

        // -- Grid --
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            GameObject matrixParent = new GameObject("matrixParent");
            List<BallClass> MatrixList = new List<BallClass>();
            for (int i = 0; i < Static.matrixD1; i++)
                for (int j = 0; j < Static.matrixD2; j++)
                    for (int h = 0; h < Static.matrixD3; h++)
                    {
                        BallClass ballS = ballFire(this.gameObject, new Vector3(
                            i * Static.ballSize * Static.ballSeperatness - Static.ballSize * Static.ballSeperatness * ((Static.matrixD1 - 1) / 2f),
                            j * Static.ballSize * Static.ballSeperatness - Static.ballSize * Static.ballSeperatness * ((Static.matrixD2 - 1) / 2f),
                            h * Static.ballSize * Static.ballSeperatness), colorZero);
                        ballS.ball.transform.parent = matrixParent.transform;
                        MatrixList.Add(ballS);
                    }
            parentObjects[matrixParent] = MatrixList;
            Vector3 mid = calculateMidPoint(MatrixList);
            foreach (BallClass ball in MatrixList)
                ball.relativePosToCenter = ball.ball.transform.position - mid;
        }

        // -- Destroy all balls --
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            foreach (BallClass bs in balls)
            {
                foreach (List<BallClass> list in parentObjects.Values)
                    if (list.Contains(bs)) list.Remove(bs);
                Destroy(bs.ball);
            }
            balls.Clear();
        }

        // -- Spark away --
        if (Keyboard.current.kKey.wasPressedThisFrame)
            sparkAway();

        // [STEP 4] Reload scene — clears everything back to a clean state
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(0);

        // [STEP 4] Semicolon — pause / unpause
        if (Keyboard.current.semicolonKey.wasPressedThisFrame)
        {
            Static.timeStep = Static.timeStep != 0 ? 0 : 0.7f;
            Time.timeScale  = Static.timeStep;
        }

        // [STEP 4] U / Y — toggle gravity multiplier (used by black holes later)
        if (Keyboard.current.uKey.wasPressedThisFrame)
            Static.gMult = Static.gMult != 0 ? 0 : 30f;

        if (Keyboard.current.yKey.wasPressedThisFrame)
            Static.gMult = Static.gMult != 0 ? 0 : 500f;

        // [STEP 4] Z — toggle near clip plane, lets you clip inside ball clusters
        if (Keyboard.current.zKey.wasPressedThisFrame)
            camera.nearClipPlane = camera.nearClipPlane == .3f ? 99999f : .3f;
    }

    void FixedUpdate()
    {
        if (Keyboard.current.jKey.isPressed)
            sparkIn();
        if (Keyboard.current.vKey.isPressed)
            returnToAllPosition();
        if (Keyboard.current.spaceKey.isPressed)
            returnToParentPosition();

        addBallForces();

        for (int i = parentObjects.Count - 1; i > -1; i--)
            if (parentObjects.ElementAt(i).Value.Count == 0)
                parentObjects.Remove(parentObjects.ElementAt(i).Key);
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
        ballClass.ball = newBall;
        ballClass.bS   = newBall.GetComponent<BallScript>();
        ballClass.rB   = newBall.GetComponent<Rigidbody>();
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

    public void sparkAway()
    {
        foreach (BallClass bStruct in balls)
            bStruct.totalForceToAdd += bStruct.ball.transform.position.normalized * 20000;
    }

    public void sparkIn()
    {
        foreach (BallClass bStruct in balls)
            bStruct.totalForceToAdd += bStruct.ball.transform.position.normalized * -100;
    }

    public void returnToAllPosition()
    {
        Vector3 midPoint = calculateMidPoint(balls);
        foreach (BallClass bStruct in balls)
        {
            Vector3 directionToAdd = (bStruct.relativeStartPos - (bStruct.ball.transform.position - midPoint));
            if (Vector3.Distance(bStruct.ball.transform.position, midPoint) > .2f)
            {
                Vector3 shouldVelocity = 3 * directionToAdd.normalized * Mathf.Pow(directionToAdd.magnitude, .4f);
                Vector3 forceAdded = spaceMult * 5 * ((shouldVelocity - bStruct.rB.linearVelocity).normalized *
                                      Mathf.Pow(Vector3.Distance(shouldVelocity, bStruct.rB.linearVelocity), 2) /
                                      Mathf.Pow(Mathf.Clamp(Vector3.Distance(midPoint, transform.position), .3f, 5000), 1 / 3));
                bStruct.totalForceToAdd += forceAdded;
            }
        }
    }

    public void returnToParentPosition()
    {
        foreach (GameObject parent in parentObjects.Keys)
        {
            List<BallClass> group = parentObjects[parent];
            if (group.Count == 0) continue;
            Vector3 midPoint = calculateMidPoint(group);
            BallClass bc = group[0];
            Vector3 originalVect = bc.relativePosToCenter;
            Vector3 currentVect  = bc.ball.transform.position - midPoint;
            if (originalVect == Vector3.zero || currentVect == Vector3.zero) continue;
            Quaternion q1 = Quaternion.LookRotation(originalVect);
            Quaternion q2 = Quaternion.LookRotation(currentVect);
            Quaternion q3 = Quaternion.Inverse(q1) * q2;
            foreach (BallClass bStruct in group)
            {
                Vector3 relativeStartWithBallSep = new Vector3(
                    bStruct.relativeStartPos.x * Static.ballSeperatness,
                    bStruct.relativeStartPos.y * Static.ballSeperatness,
                    bStruct.relativeStartPos.z * Static.ballSeperatness);
                Vector3 directionToAdd = (q3 * relativeStartWithBallSep) - (bStruct.ball.transform.position - midPoint);
                if (Vector3.Distance(bStruct.ball.transform.position, midPoint) > .01f)
                {
                    Vector3 shouldVelocity = 5 * directionToAdd.normalized * Mathf.Pow(directionToAdd.magnitude, spaceAccel);
                    Vector3 forceAdded = spaceMult * ((shouldVelocity - bStruct.rB.linearVelocity).normalized *
                                          Mathf.Pow(Vector3.Distance(shouldVelocity, bStruct.rB.linearVelocity), 2) /
                                          Mathf.Pow(Mathf.Clamp(Vector3.Distance(midPoint, transform.position), .3f, 500), 1 / 3));
                    if (forceAdded.magnitude > 3000)
                        forceAdded = forceAdded.normalized * 3000;
                    bStruct.totalForceToAdd += forceAdded;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // UTILITIES
    // -------------------------------------------------------------------------

    public void rotateParent(GameObject parent, Quaternion rotation)
    {
        parent.transform.rotation = rotation;
    }

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

    public void updateBallColor() { }
}