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
    public List<GameObject> ballPrefabs;

    public Vector3 mousePos;
    public Camera camera;

    private bool ballOnCool;
    public float spaceMult  = 0.008f;
    public float spaceAccel = 0.4f;

    public Material ballDefaultMat;
    public Material ballTransparentMat;

    private string colorPropName =>
        UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
            ? "_BaseColor" : "_Color";

    private string smoothnessPropName =>
        UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
            ? "_Smoothness" : "_Glossiness";

    private GeometryLaunchManager geometryLaunchManager;
    private Gatling gatling;

    // [STEP 8] Camera control references
    // DragMouseOrbit and MouseLook should be components on the Main Camera GameObject
    public DragMouseOrbit dragMouseOrbitRef;
    public MouseLook mouseLookRef;
    public GameObject camTarget; // optional — orbit target; null = orbits around origin
    private CameraManager cameraManager;
    private int cameraSetting = 0;

    // -------------------------------------------------------------------------
    // LIFECYCLE
    // -------------------------------------------------------------------------

    void Start()
    {
        balls = new List<BallClass>();
        BallInstYesPrefab = spherePrefab;
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        parentObjects = new Dictionary<GameObject, List<BallClass>>();

        geometryLaunchManager = GetComponent<GeometryLaunchManager>();
        gatling = GetComponent<Gatling>();

        GeometryLaunchManager.bouncyShootRef = this;
        ImageCreater.bouncyShootRef          = this;
        MeshVertManager.bouncyShootRef       = this;
        if (gatling != null) gatling.bouncyShootRef = this;

        // [STEP 8] Wire camera components — both scripts live on the Main Camera
        mouseLookRef      = camera.GetComponent<MouseLook>();
        dragMouseOrbitRef = camera.GetComponent<DragMouseOrbit>();

        // CameraManager lives on a separate GameObject; null-safe if not yet set up
        cameraManager = FindFirstObjectByType<CameraManager>();

        Static.ballColorR = 190;
        Static.ballColorG = 190;
        Static.ballColorB = 190;
        updateBallColor();
    }

    // -------------------------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------------------------

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

        // -- Reload scene --
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(0);

        // -- Pause / unpause --
        if (Keyboard.current.semicolonKey.wasPressedThisFrame)
        {
            Static.timeStep = Static.timeStep != 0 ? 0 : 0.7f;
            Time.timeScale  = Static.timeStep;
        }

        // -- Toggle gMult --
        if (Keyboard.current.uKey.wasPressedThisFrame)
            Static.gMult = Static.gMult != 0 ? 0 : 30f;
        if (Keyboard.current.yKey.wasPressedThisFrame)
            Static.gMult = Static.gMult != 0 ? 0 : 500f;

        // -- Toggle near clip plane --
        if (Keyboard.current.zKey.wasPressedThisFrame)
            camera.nearClipPlane = camera.nearClipPlane == .3f ? 99999f : .3f;

        // -- Color presets (Shift + number) --
        bool shift = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        if (shift)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) { Static.ballColorR=190; Static.ballColorG=190; Static.ballColorB=190; updateBallColor(); }
            if (Keyboard.current.digit2Key.wasPressedThisFrame) { Static.ballColorR=0;   Static.ballColorG=0;   Static.ballColorB=245; updateBallColor(); }
            if (Keyboard.current.digit3Key.wasPressedThisFrame) { Static.ballColorR=160; Static.ballColorG=0;   Static.ballColorB=0;   updateBallColor(); }
            if (Keyboard.current.digit4Key.wasPressedThisFrame) { Static.ballColorR=0;   Static.ballColorG=140; Static.ballColorB=0;   updateBallColor(); }
            if (Keyboard.current.digit5Key.wasPressedThisFrame) { Static.ballColorR=0;   Static.ballColorG=200; Static.ballColorB=200; updateBallColor(); }
            if (Keyboard.current.digit6Key.wasPressedThisFrame) { Static.ballColorR=50;  Static.ballColorG=0;   Static.ballColorB=200; updateBallColor(); }
        }

        // -- H: cycle ball shape --
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            Static.currentBallShape++;
            if (ballPrefabs != null && ballPrefabs.Count > 0)
                BallInstYesPrefab = ballPrefabs[Static.currentBallShape % ballPrefabs.Count];
        }

        // -- S: circle --
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            GameObject parent = new GameObject("circleParent");
            parent.transform.position = transform.position;
            parentObjects[parent] = GeometryLaunchManager.fireCircle(parent, 200, 10);
            rotateParent(parent, transform.rotation);
            Vector3 mid = calculateMidPoint(parentObjects[parent]);
            foreach (BallClass ball in parentObjects[parent])
                ball.relativePosToCenter = ball.ball.transform.position - mid;
        }

        // -- A: double cone --
        if (Keyboard.current.aKey.wasPressedThisFrame && geometryLaunchManager != null)
        {
            GameObject parent = new GameObject("doubleConeParent");
            parent.transform.position = transform.position;
            parentObjects[parent] = geometryLaunchManager.fireCircleDoubleCone(parent);
            rotateParent(parent, transform.rotation);
        }

        // -- D: vert model --
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (MeshVertManager.vertModelDict != null && MeshVertManager.vertModelDict.Count > 0)
            {
                GameObject parent = new GameObject("vertModelParent");
                parent.transform.position = transform.position;
                parentObjects[parent] = MeshVertManager.fireVertModel(parent);
                rotateParent(parent, transform.rotation);
                Vector3 mid = calculateMidPoint(parentObjects[parent]);
                foreach (BallClass ball in parentObjects[parent])
                    ball.relativePosToCenter = ball.ball.transform.position - mid;
            }
            else
                Debug.LogWarning("D key: no vert models loaded. Add prefabs to Assets/Resources/VertModels.");
        }

        // -- I: image --
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (Static.currentImage != null && Static.imageDivideBy != 0)
            {
                GameObject parent = new GameObject("imageParent");
                parent.transform.position = transform.position;
                parentObjects[parent] = ImageCreater.fireImage(Static.currentImage, Static.imageDivideBy, parent);
                Vector3 mid = calculateMidPoint(parentObjects[parent]);
                foreach (BallClass ball in parentObjects[parent])
                    ball.relativePosToCenter = ball.ball.transform.position - mid;
            }
            else
                Debug.LogWarning("I key: Static.currentImage is null. Assign a Texture2D to Static.currentImage.");
        }

        // -- B: gatling (hold) --
        if (Keyboard.current.bKey.wasPressedThisFrame && gatling != null)
        {
            gatling.firingOn(0f,   true);
            gatling.firingOn(.33f, true);
            gatling.firingOn(.66f, true);
        }
        if (Keyboard.current.bKey.wasReleasedThisFrame && gatling != null)
            gatling.firingOff();

        // -- M: circle gatling toggle --
        if (Keyboard.current.mKey.wasPressedThisFrame && gatling != null)
            gatling.firingCircleToggle();
    }

    // -------------------------------------------------------------------------
    // FIXED UPDATE
    // -------------------------------------------------------------------------

    void FixedUpdate()
    {
        if (Keyboard.current.jKey.isPressed)     sparkIn();
        if (Keyboard.current.vKey.isPressed)     returnToAllPosition();
        if (Keyboard.current.spaceKey.isPressed) returnToParentPosition();

        addBallForces();

        for (int i = parentObjects.Count - 1; i > -1; i--)
            if (parentObjects.ElementAt(i).Value.Count == 0)
                parentObjects.Remove(parentObjects.ElementAt(i).Key);
    }

    // -------------------------------------------------------------------------
    // LATE UPDATE — camera controls                                   [STEP 8]
    // -------------------------------------------------------------------------

    void LateUpdate()
    {
        // Reset look orientation when shift is pressed/released so there's
        // no jump when switching between orbit and free-look modes
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            mouseLookRef?.setRotationToCurrent();
        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
            mouseLookRef?.setRotationToCurrent();

        // Arrow keys: up/down zoom, left/right orbit
        if (Keyboard.current.upArrowKey.isPressed)
        {
            camera.gameObject.transform.position *= .997f;
            cameraManager?.zoom(.997f);
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            camera.gameObject.transform.position *= 1.003f;
            cameraManager?.zoom(1.003f);
        }

        Vector3 targetPos = camTarget ? camTarget.transform.position : Vector3.zero;
        float dist = Vector3.Distance(camera.gameObject.transform.position, targetPos);

        if (Keyboard.current.leftArrowKey.isPressed)
            dragMouseOrbitRef?.rotateLeft(dist, targetPos);
        if (Keyboard.current.rightArrowKey.isPressed)
            dragMouseOrbitRef?.rotateRight(dist, targetPos);

        // Right mouse button: reset look on press, then orbit (shift) or free-look
        if (Mouse.current.rightButton.wasPressedThisFrame)
            mouseLookRef?.setRotationToCurrent();

        if (Mouse.current.rightButton.isPressed)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
                dragMouseOrbitRef?.updateMouseOrbit(dist, targetPos);
            else
                mouseLookRef?.updateMouseLook();
        }

        // Scroll wheel zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f && dist > 1f)
        {
            camera.gameObject.transform.position *= .99f;
            cameraManager?.zoom(.99f);
        }
        else if (scroll < 0f && dist < 5000f)
        {
            camera.gameObject.transform.position *= 1.01f;
            cameraManager?.zoom(1.01f);
        }

        // C key — toggle color cycling on all balls
        if (Keyboard.current.cKey.wasPressedThisFrame)
            foreach (BallClass bC in balls)
                bC.bS.colorChangeToggle();

        // Quote key — cycle extra camera modes (only if CameraManager is set up)
        if (Keyboard.current.quoteKey.wasPressedThisFrame && cameraManager != null)
        {
            cameraSetting++;
            cameraManager.switchCam(cameraSetting % CameraManager.NUMBERCAMTYPES);
        }
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
            newBall.GetComponent<MeshRenderer>().material.SetColor(colorPropName, color);

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
    // COLOR & MATERIAL
    // -------------------------------------------------------------------------

    public void updateBallColor()
    {
        Color col = new Color(
            Static.ballColorR / 255f,
            Static.ballColorG / 255f,
            Static.ballColorB / 255f,
            Static.ballColorA);
        if (ballDefaultMat)     ballDefaultMat.SetColor(colorPropName, col);
        if (ballTransparentMat) ballTransparentMat.SetColor(colorPropName, col);
    }

    public void setColorR(float r) { Static.ballColorR = Mathf.RoundToInt(r); updateBallColor(); }
    public void setColorG(float g) { Static.ballColorG = Mathf.RoundToInt(g); updateBallColor(); }
    public void setColorB(float b) { Static.ballColorB = Mathf.RoundToInt(b); updateBallColor(); }
    public void setColorA(float a) { Static.ballColorA = a;                   updateBallColor(); }

    public void setMetallic(float v)
    {
        Static.ballMetallic = v;
        if (ballDefaultMat)     ballDefaultMat.SetFloat("_Metallic", v);
        if (ballTransparentMat) ballTransparentMat.SetFloat("_Metallic", v);
    }

    public void setSmoothness(float v)
    {
        Static.ballSmoothness = v;
        if (ballDefaultMat)     ballDefaultMat.SetFloat(smoothnessPropName, v);
        if (ballTransparentMat) ballTransparentMat.SetFloat(smoothnessPropName, v);
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
}