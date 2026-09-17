using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DefaultNamespace;

// Suggested slider ranges (set in inspector):
//   ballSize:         0.1  – 10
//   ballForce:        0    – 2000
//   ballVelocity:     0    – 100
//   ballMass:         0.1  – 10
//   timeStep:         0    – 1
//   colorR/G/B:       0    – 255
//   colorA:           0    – 1
//   metallic:         0    – 1
//   smoothness:       0    – 1
//   ballSeperatness:  0.5  – 20
//   gMult:            0    – 50000
//   gExp:             0.1  – 5
//   imageDivideBy:    1    – 20   (whole numbers)
//   matrixD1/D2/D3:   1    – 100  (whole numbers)

public class UIManager : MonoBehaviour
{
    [Header("Core reference")]
    public BouncyShoot bouncyShoot;
    public GameObject  canvas;

    // ── LIVE DISPLAY ─────────────────────────────────────────────────────────
    [Header("Live display (TMP_Text)")]
    public TMP_Text frameRateText;
    public TMP_Text ballCountText;

    // ── SLIDER LABELS ─────────────────────────────────────────────────────────
    [Header("Slider labels (TMP_Text)")]
    public TMP_Text ballSizeText;
    public TMP_Text ballForceText;
    public TMP_Text ballVelocityText;
    public TMP_Text ballMassText;
    public TMP_Text timeStepText;
    public TMP_Text ballSeperatenessText;
    public TMP_Text gMultText;
    public TMP_Text gExpText;
    public TMP_Text imageDivideByText;
    public TMP_Text matrixD1Text;
    public TMP_Text matrixD2Text;
    public TMP_Text matrixD3Text;

    // ── SLIDERS ───────────────────────────────────────────────────────────────
    [Header("Ball")]
    public Slider ballSizeSlider;
    public Slider ballForceSlider;
    public Slider ballVelocitySlider;
    public Slider ballMassSlider;
    public Slider ballSeperatenessSlider;

    [Header("Color & material")]
    public Slider colorRSlider;
    public Slider colorGSlider;
    public Slider colorBSlider;
    public Slider colorASlider;
    public Slider metallicSlider;
    public Slider smoothnessSlider;

    [Header("Physics & gravity")]
    public Slider gMultSlider;
    public Slider gExpSlider;
    public Slider timeStepSlider;

    [Header("Grid & image")]
    public Slider matrixD1Slider;
    public Slider matrixD2Slider;
    public Slider matrixD3Slider;
    public Slider imageDivideBySlider;

    // ── TOGGLES ───────────────────────────────────────────────────────────────
    [Header("Toggles")]
    public Toggle gravityToggle;
    public Toggle wallsToggle;
    public Toggle velocityDirectToggle;

    [Header("Scene objects")]
    public GameObject walls;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    void Start()
    {
        // Ball
        InitSlider(ballSizeSlider,         Static.ballSize,         _ => setBallSize());
        InitSlider(ballForceSlider,        Static.ballForce,        _ => setBallForce());
        InitSlider(ballVelocitySlider,     Static.ballVelocity,     _ => setBallVelocity());
        InitSlider(ballMassSlider,         Static.ballMass,         _ => setBallMass());
        InitSlider(ballSeperatenessSlider, Static.ballSeperatness,  _ => setBallSeperateness());

        // Color & material
        InitSlider(colorRSlider,     Static.ballColorR,    v => { if (bouncyShoot) bouncyShoot.setColorR(v); });
        InitSlider(colorGSlider,     Static.ballColorG,    v => { if (bouncyShoot) bouncyShoot.setColorG(v); });
        InitSlider(colorBSlider,     Static.ballColorB,    v => { if (bouncyShoot) bouncyShoot.setColorB(v); });
        InitSlider(colorASlider,     Static.ballColorA,    v => { if (bouncyShoot) bouncyShoot.setColorA(v); });
        InitSlider(metallicSlider,   Static.ballMetallic,  v => { if (bouncyShoot) bouncyShoot.setMetallic(v); });
        InitSlider(smoothnessSlider, Static.ballSmoothness,v => { if (bouncyShoot) bouncyShoot.setSmoothness(v); });

        // Physics & gravity
        InitSlider(gMultSlider,   Static.gMult,   _ => setGMult());
        InitSlider(gExpSlider,    Static.gExp,    _ => setGExp());
        InitSlider(timeStepSlider,Static.timeStep,_ => setTimeStep());

        // Grid & image (whole numbers)
        InitIntSlider(matrixD1Slider,      Static.matrixD1,      _ => setMatrixD1());
        InitIntSlider(matrixD2Slider,      Static.matrixD2,      _ => setMatrixD2());
        InitIntSlider(matrixD3Slider,      Static.matrixD3,      _ => setMatrixD3());
        InitIntSlider(imageDivideBySlider, Static.imageDivideBy, _ => setImageDivideBy());

        // Toggles
        InitToggle(gravityToggle,        Static.toggleGravity,        _ => toggleGravity());
        InitToggle(wallsToggle,          Static.toggleWalls,          _ => toggleWalls());
        InitToggle(velocityDirectToggle, Static.velocityDirectToggle, _ => toggleVelocityDirect());

        // Set initial label text
        setBallSize(); setBallForce(); setBallVelocity(); setBallMass();
        setBallSeperateness(); setGMult(); setGExp(); setTimeStep();
        setMatrixD1(); setMatrixD2(); setMatrixD3(); setImageDivideBy();
    }

    void Update()
    {
        if (frameRateText)
            frameRateText.text = Mathf.RoundToInt(1f / Time.unscaledDeltaTime) + " fps";
        if (ballCountText)
            ballCountText.text = (BouncyShoot.balls != null ? BouncyShoot.balls.Count : 0) + " balls";

        // O key — toggle canvas visibility
        if (UnityEngine.InputSystem.Keyboard.current.oKey.wasPressedThisFrame && canvas)
            canvas.SetActive(!canvas.activeSelf);
    }

    // =========================================================================
    // SETTERS
    // =========================================================================

    public void setBallSize()
    {
        if (!ballSizeSlider) return;
        Static.ballSize = ballSizeSlider.value;
        Static.ballCoolTime = Static.ballSize / 20f;
        if (ballSizeText) ballSizeText.text = "size: " + Static.ballSize.ToString("F1");
    }

    public void setBallForce()
    {
        if (!ballForceSlider) return;
        Static.ballForce = ballForceSlider.value;
        if (ballForceText) ballForceText.text = "force: " + Static.ballForce.ToString("F0");
    }

    public void setBallVelocity()
    {
        if (!ballVelocitySlider) return;
        Static.ballVelocity = ballVelocitySlider.value;
        if (ballVelocityText) ballVelocityText.text = "vel: " + Static.ballVelocity.ToString("F0");
    }

    public void setBallMass()
    {
        if (!ballMassSlider) return;
        Static.ballMass = ballMassSlider.value;
        if (ballMassText) ballMassText.text = "mass: " + Static.ballMass.ToString("F2");
    }

    public void setBallSeperateness()
    {
        if (!ballSeperatenessSlider) return;
        Static.ballSeperatness = ballSeperatenessSlider.value;
        if (ballSeperatenessText) ballSeperatenessText.text = "sep: " + Static.ballSeperatness.ToString("F1");
    }

    public void setGMult()
    {
        if (!gMultSlider) return;
        Static.gMult = gMultSlider.value;
        if (gMultText) gMultText.text = "gMult: " + Static.gMult.ToString("F0");
    }

    public void setGExp()
    {
        if (!gExpSlider) return;
        Static.gExp = gExpSlider.value;
        if (gExpText) gExpText.text = "gExp: " + Static.gExp.ToString("F2");
    }

    public void setTimeStep()
    {
        if (!timeStepSlider) return;
        Static.timeStep  = timeStepSlider.value;
        Time.timeScale   = Static.timeStep;
        Time.fixedDeltaTime = 0.02f * Static.timeStep; // keeps physics updates proportional
        if (timeStepText) timeStepText.text = "time: " + Static.timeStep.ToString("F2");
    }

    public void setMatrixD1()
    {
        if (!matrixD1Slider) return;
        Static.matrixD1 = Mathf.RoundToInt(matrixD1Slider.value);
        if (matrixD1Text) matrixD1Text.text = "D1: " + Static.matrixD1;
    }

    public void setMatrixD2()
    {
        if (!matrixD2Slider) return;
        Static.matrixD2 = Mathf.RoundToInt(matrixD2Slider.value);
        if (matrixD2Text) matrixD2Text.text = "D2: " + Static.matrixD2;
    }

    public void setMatrixD3()
    {
        if (!matrixD3Slider) return;
        Static.matrixD3 = Mathf.RoundToInt(matrixD3Slider.value);
        if (matrixD3Text) matrixD3Text.text = "D3: " + Static.matrixD3;
    }

    public void setImageDivideBy()
    {
        if (!imageDivideBySlider) return;
        Static.imageDivideBy = Mathf.RoundToInt(imageDivideBySlider.value);
        if (imageDivideByText) imageDivideByText.text = "imgDiv: " + Static.imageDivideBy;
    }

    // =========================================================================
    // TOGGLES
    // =========================================================================

    public void toggleGravity()
    {
        if (!gravityToggle) return;
        Static.toggleGravity = gravityToggle.isOn;
        if (BouncyShoot.balls != null)
            foreach (BallClass ball in BouncyShoot.balls)
                ball.rB.useGravity = Static.toggleGravity;
    }

    public void toggleWalls()
    {
        if (!wallsToggle) return;
        Static.toggleWalls = wallsToggle.isOn;
        if (walls) walls.SetActive(Static.toggleWalls);
    }

    public void toggleVelocityDirect()
    {
        if (!velocityDirectToggle) return;
        Static.velocityDirectToggle = velocityDirectToggle.isOn;
        if (ballForceSlider)    ballForceSlider.gameObject.SetActive(!Static.velocityDirectToggle);
        if (ballVelocitySlider) ballVelocitySlider.gameObject.SetActive(Static.velocityDirectToggle);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private void InitSlider(Slider s, float startValue, UnityEngine.Events.UnityAction<float> callback)
    {
        if (!s) return;
        s.value = startValue;
        s.onValueChanged.AddListener(callback);
    }

    private void InitIntSlider(Slider s, int startValue, UnityEngine.Events.UnityAction<float> callback)
    {
        if (!s) return;
        s.wholeNumbers = true;
        s.value = startValue;
        s.onValueChanged.AddListener(callback);
    }

    private void InitToggle(Toggle t, bool startValue, UnityEngine.Events.UnityAction<bool> callback)
    {
        if (!t) return;
        t.isOn = startValue;
        t.onValueChanged.AddListener(callback);
    }
}