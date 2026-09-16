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
    // -------------------------------------------------------------------------
    // STATIC STATE
    // -------------------------------------------------------------------------
    public static readonly Color colorZero = Color.clear;
    public static List<BallClass> balls;
    public static Dictionary<GameObject, List<BallClass>> parentObjects;

    // -------------------------------------------------------------------------
    // INSPECTOR FIELDS
    // -------------------------------------------------------------------------
    [Header("Ball Prefabs")]
    public GameObject BallInstYesPrefab;   // GPU-instanced, no per-ball color
    public GameObject BallInstNoPrefab;    // non-instanced, supports per-ball color
    public GameObject spherePrefab;        // assigned to BallInstYesPrefab on Start
    public List<GameObject> ballPrefabs;   // H key cycles through these

    [Header("Materials")]
    public Material ballDefaultMat;        // shared material for instanced balls
    public Material ballTransparentMat;    // shared material for colored balls

    [Header("Camera")]
    public Camera camera;
    public DragMouseOrbit dragMouseOrbitRef;
    public MouseLook mouseLookRef;
    public GameObject camTarget;           // orbit target; null = orbits origin
    public CameraManager cameraManager;
    

    [Header("Tuning")]
    public float spaceMult  = 0.008f;
    public float spaceAccel = 0.4f;

    // -------------------------------------------------------------------------
    // PRIVATE
    // -------------------------------------------------------------------------
    private bool ballOnCool;
    private int  cameraSetting = 0;
    private GeometryLaunchManager geometryLaunchManager;
    private Gatling gatling;

    // Auto-detects URP vs Built-in pipeline for correct material property names
    private string colorPropName =>
        UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
            ? "_BaseColor" : "_Color";
    private string smoothnessPropName =>
        UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
            ? "_Smoothness" : "_Glossiness";

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    void Start()
    {
        balls         = new List<BallClass>();
        parentObjects = new Dictionary<GameObject, List<BallClass>>();

        BallInstYesPrefab = spherePrefab;

        geometryLaunchManager = GetComponent<GeometryLaunchManager>();
        gatling               = GetComponent<Gatling>();

        GeometryLaunchManager.bouncyShootRef = this;
        ImageCreater.bouncyShootRef          = this;
        MeshVertManager.bouncyShootRef       = this;
        if (gatling != null) gatling.bouncyShootRef = this;

        // Default ball color: light gray
        Static.ballColorR = 190;
        Static.ballColorG = 190;
        Static.ballColorB = 190;
        updateBallColor();

        ModelManager.bouncyShootRef = this;

    }

    // =========================================================================
    // UPDATE — input & spawning
    // =========================================================================

    void Update()
    {
        // -- Aim shooter at mouse --
        Vector2 mouseScreenPos  = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos   = camera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 1f));
        transform.LookAt(mouseWorldPos);

        // -- F (held): fire single ball on cooldown --
        if (Keyboard.current.fKey.isPressed)
            if (!ballOnCool)
                StartCoroutine(ballCD(Static.ballCoolTime));

        // -- G: spawn grid --
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

        // -- T: destroy all balls --
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

        // -- K: spark away --
        if (Keyboard.current.kKey.wasPressedThisFrame)
            sparkAway();

        // -- R: reload scene --
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(0);

        // -- Semicolon: pause / unpause --
        if (Keyboard.current.semicolonKey.wasPressedThisFrame)
        {
            Static.timeStep = Static.timeStep != 0 ? 0 : 0.7f;
            Time.timeScale  = Static.timeStep;
        }

        // to:
        if (Keyboard.current.uKey.wasPressedThisFrame)
            Static.gMult = Static.gMult != 0 ? 0 : 1000f;
        if (Keyboard.current.yKey.wasPressedThisFrame)
            Static.gMult = Static.gMult != 0 ? 0 : 5000f;

        // -- Z: toggle near clip plane (clip inside ball clusters) --
        if (Keyboard.current.zKey.wasPressedThisFrame)
            camera.nearClipPlane = camera.nearClipPlane == .3f ? 99999f : .3f;

        // -- Shift + 1-6: color presets --
        bool shift = Keyboard.current.leftShiftKey.isPressed
                  || Keyboard.current.rightShiftKey.isPressed;
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

        // -- S: fire circle --
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

        // -- A: fire double cone (spawns over ~2 seconds via coroutine) --
        if (Keyboard.current.aKey.wasPressedThisFrame && geometryLaunchManager != null)
        {
            GameObject parent = new GameObject("doubleConeParent");
            parent.transform.position = transform.position;
            parentObjects[parent] = geometryLaunchManager.fireCircleDoubleCone(parent);
            rotateParent(parent, transform.rotation);
        }

        // -- D: fire vert model --
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
                Debug.LogWarning("D key: no vert models loaded — add prefabs to Assets/Resources/VertModels.");
        }

        // -- I: fire image --
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (Static.currentImage != null && Static.imageDivideBy != 0)
            {
                GameObject parent = new GameObject("imageParent");
                parent.transform.position = transform.position;
                parentObjects[parent] = ImageCreater.fireImage(
                    Static.currentImage, Static.imageDivideBy, parent);
                Vector3 mid = calculateMidPoint(parentObjects[parent]);
                foreach (BallClass ball in parentObjects[parent])
                    ball.relativePosToCenter = ball.ball.transform.position - mid;
            }
            else
                Debug.LogWarning("I key: Static.currentImage is null — assign a Texture2D first.");
        }

        // -- B (hold): gatling gun --
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

            // L — fire current shape
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            if (ModelManager.shapeDict != null && ModelManager.shapeDict.Count > 0)
            {
                GameObject parent = new GameObject("shapeParent");
                parent.transform.position = transform.position;
                parentObjects[parent] = ModelManager.fireShape(parent);
                rotateParent(parent, transform.rotation);
                Vector3 mid = calculateMidPoint(parentObjects[parent]);
                foreach (BallClass ball in parentObjects[parent])
                    ball.relativePosToCenter = ball.ball.transform.position - mid;
                Debug.Log($"Fired shape: {ModelManager.currentShapeName()}");
            }
        }

        // Comma / Period — cycle through shapes
        if (Keyboard.current.commaKey.wasPressedThisFrame && ModelManager.shapeNames != null)
        {
            Static.modelIndex--;
            if (Static.modelIndex < 0) Static.modelIndex = ModelManager.shapeNames.Length - 1;
            Debug.Log($"Shape: {ModelManager.currentShapeName()}");
        }
        if (Keyboard.current.periodKey.wasPressedThisFrame && ModelManager.shapeNames != null)
        {
            Static.modelIndex = (Static.modelIndex + 1) % ModelManager.shapeNames.Length;
            Debug.Log($"Shape: {ModelManager.currentShapeName()}");
        }
    }

    // =========================================================================
    // FIXED UPDATE — continuous forces
    // =========================================================================

    void FixedUpdate()
    {
        if (Keyboard.current.jKey.isPressed)     sparkIn();
        if (Keyboard.current.vKey.isPressed)     returnToAllPosition();
        if (Keyboard.current.spaceKey.isPressed) returnToParentPosition();

        addBallForces();

        // Remove parent groups that have run out of balls
        for (int i = parentObjects.Count - 1; i > -1; i--)
            if (parentObjects.ElementAt(i).Value.Count == 0)
                parentObjects.Remove(parentObjects.ElementAt(i).Key);
    }

    // =========================================================================
    // LATE UPDATE — camera controls
    // =========================================================================

    void LateUpdate()
    {
        // Sync MouseLook orientation when switching modes so there's no jump
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
            Keyboard.current.leftShiftKey.wasReleasedThisFrame)
            mouseLookRef?.setRotationToCurrent();

        // Up/Down arrows: zoom
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

        // Orbit target (null = world origin)
        Vector3 targetPos = camTarget ? camTarget.transform.position : Vector3.zero;

        // Left/Right arrows: orbit rotate
        if (Keyboard.current.leftArrowKey.isPressed)
            dragMouseOrbitRef?.rotateLeft(targetPos);
        if (Keyboard.current.rightArrowKey.isPressed)
            dragMouseOrbitRef?.rotateRight(targetPos);

        // Right mouse: sync look on first press, then orbit (shift) or free-look
        if (Mouse.current.rightButton.wasPressedThisFrame)
            mouseLookRef?.setRotationToCurrent();

        if (Mouse.current.rightButton.isPressed)
        {
            bool shiftHeld = Keyboard.current.leftShiftKey.isPressed
                          || Keyboard.current.rightShiftKey.isPressed;
            if (shiftHeld)
                dragMouseOrbitRef?.updateMouseOrbit(targetPos);
            else
                mouseLookRef?.updateMouseLook();
        }

        // Scroll wheel: zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        float dist   = Vector3.Distance(camera.gameObject.transform.position, targetPos);
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

        // C: toggle color cycling on all balls
        if (Keyboard.current.cKey.wasPressedThisFrame)
            foreach (BallClass bC in balls)
                bC.bS.colorChangeToggle();

        // Quote: cycle extra camera modes
        if (Keyboard.current.quoteKey.wasPressedThisFrame && cameraManager != null)
        {
            cameraSetting++;
            cameraManager.switchCam(cameraSetting % CameraManager.NUMBERCAMTYPES);
        }
    }

    // =========================================================================
    // BALL SPAWNING
    // =========================================================================

    public BallClass ballFire(GameObject shootingObj, Vector3 addedVect, Color color)
    {
        GameObject newBall;
        if (color == Color.clear || color == colorZero || color == Color.black)
            newBall = Instantiate(BallInstYesPrefab,
                shootingObj.transform.position + addedVect, Quaternion.identity);
        else
            newBall = Instantiate(BallInstNoPrefab,
                shootingObj.transform.position + addedVect, Quaternion.identity);

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

    // =========================================================================
    // FORCES
    // =========================================================================

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
            Vector3 directionToAdd = bStruct.relativeStartPos -
                                     (bStruct.ball.transform.position - midPoint);
            if (Vector3.Distance(bStruct.ball.transform.position, midPoint) > .2f)
            {
                Vector3 shouldVelocity = 3 * directionToAdd.normalized *
                    Mathf.Pow(directionToAdd.magnitude, .4f);
                // Note: 1/3 is integer division (= 0), Pow term = 1 — matches original
                Vector3 forceAdded = spaceMult * 5 *
                    ((shouldVelocity - bStruct.rB.linearVelocity).normalized *
                     Mathf.Pow(Vector3.Distance(shouldVelocity, bStruct.rB.linearVelocity), 2) /
                     Mathf.Pow(Mathf.Clamp(
                         Vector3.Distance(midPoint, transform.position), .3f, 5000), 1 / 3));
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

            Vector3 midPoint    = calculateMidPoint(group);
            BallClass bc        = group[0];
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

                Vector3 directionToAdd = (q3 * relativeStartWithBallSep) -
                                         (bStruct.ball.transform.position - midPoint);

                if (Vector3.Distance(bStruct.ball.transform.position, midPoint) > .01f)
                {
                    Vector3 shouldVelocity = 5 * directionToAdd.normalized *
                        Mathf.Pow(directionToAdd.magnitude, spaceAccel);
                    // Note: 1/3 integer division = 0, Pow term = 1 — matches original
                    Vector3 forceAdded = spaceMult *
                        ((shouldVelocity - bStruct.rB.linearVelocity).normalized *
                         Mathf.Pow(Vector3.Distance(shouldVelocity, bStruct.rB.linearVelocity), 2) /
                         Mathf.Pow(Mathf.Clamp(
                             Vector3.Distance(midPoint, transform.position), .3f, 500), 1 / 3));
                    if (forceAdded.magnitude > 3000)
                        forceAdded = forceAdded.normalized * 3000;
                    bStruct.totalForceToAdd += forceAdded;
                }
            }
        }
    }

    // =========================================================================
    // COLOR & MATERIAL
    // =========================================================================

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

    // =========================================================================
    // UTILITIES
    // =========================================================================

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