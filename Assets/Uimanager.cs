// UIManager.cs — Step 6
//
// SETUP INSTRUCTIONS:
//   1. Add this script to any GameObject in your scene (e.g. a UIManager GameObject)
//   2. Make sure your project has TextMeshPro installed (it is by default in Unity 6)
//   3. In your Canvas, create the UI elements you want (Sliders, TMP_Text, Toggles)
//   4. Drag the BouncyShoot GameObject into the "Bouncy Shoot" slot in the inspector
//   5. Drag each UI element into its matching slot — any slots left empty are simply skipped
//   6. O key toggles the canvas on/off (same as the original)

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DefaultNamespace;

public class UIManager : MonoBehaviour
{
    [Header("Core reference")]
    public BouncyShoot bouncyShoot;
    public GameObject canvas;

    // -------------------------------------------------------------------------
    // DISPLAY TEXT
    // -------------------------------------------------------------------------
    [Header("Live display (TMP_Text)")]
    public TMP_Text frameRateText;   // shows current FPS
    public TMP_Text ballCountText;   // shows number of balls alive

    [Header("Slider labels (TMP_Text)")]
    public TMP_Text ballSizeText;
    public TMP_Text ballForceText;
    public TMP_Text ballVelocityText;
    public TMP_Text ballMassText;
    public TMP_Text timeStepText;

    // -------------------------------------------------------------------------
    // SLIDERS
    // -------------------------------------------------------------------------
    [Header("Sliders")]
    public Slider ballSizeSlider;
    public Slider ballForceSlider;
    public Slider ballVelocitySlider;
    public Slider ballMassSlider;
    public Slider timeStepSlider;
    public Slider colorRSlider;      // range 0–255
    public Slider colorGSlider;
    public Slider colorBSlider;
    public Slider colorASlider;      // range 0–1
    public Slider metallicSlider;    // range 0–1
    public Slider smoothnessSlider;  // range 0–1

    // -------------------------------------------------------------------------
    // TOGGLES
    // -------------------------------------------------------------------------
    [Header("Toggles")]
    public Toggle gravityToggle;
    public Toggle wallsToggle;
    public Toggle velocityDirectToggle;

    [Header("Scene objects")]
    public GameObject walls;

    // -------------------------------------------------------------------------
    // LIFECYCLE
    // -------------------------------------------------------------------------

    void Start()
    {
        // -- Sliders: set initial values from Static, then add listeners --
        InitSlider(ballSizeSlider,     Static.ballSize,            _ => setBallSize());
        InitSlider(ballForceSlider,    Static.ballForce,           _ => setBallForce());
        InitSlider(ballVelocitySlider, Static.ballVelocity,        _ => setBallVelocity());
        InitSlider(ballMassSlider,     Static.ballMass,            _ => setBallMass());
        InitSlider(timeStepSlider,     Static.timeStep,            _ => setTimeStep());
        InitSlider(colorRSlider,       Static.ballColorR,          v => { if (bouncyShoot) bouncyShoot.setColorR(v); });
        InitSlider(colorGSlider,       Static.ballColorG,          v => { if (bouncyShoot) bouncyShoot.setColorG(v); });
        InitSlider(colorBSlider,       Static.ballColorB,          v => { if (bouncyShoot) bouncyShoot.setColorB(v); });
        InitSlider(colorASlider,       Static.ballColorA,          v => { if (bouncyShoot) bouncyShoot.setColorA(v); });
        InitSlider(metallicSlider,     Static.ballMetallic,        v => { if (bouncyShoot) bouncyShoot.setMetallic(v); });
        InitSlider(smoothnessSlider,   Static.ballSmoothness,      v => { if (bouncyShoot) bouncyShoot.setSmoothness(v); });

        // -- Toggles --
        InitToggle(gravityToggle,         Static.toggleGravity,       _ => toggleGravity());
        InitToggle(wallsToggle,           Static.toggleWalls,         _ => toggleWalls());
        InitToggle(velocityDirectToggle,  Static.velocityDirectToggle,_ => toggleVelocityDirect());

        // -- Set initial label text --
        setBallSize(); setBallForce(); setBallVelocity(); setBallMass(); setTimeStep();
    }

    void Update()
    {
        // Live displays (use unscaled delta so FPS still shows when paused)
        if (frameRateText)
            frameRateText.text = Mathf.RoundToInt(1f / Time.unscaledDeltaTime) + " fps";
        if (ballCountText)
            ballCountText.text = (BouncyShoot.balls != null ? BouncyShoot.balls.Count : 0) + " balls";

        // O key — toggle canvas visibility
        if (UnityEngine.InputSystem.Keyboard.current.oKey.wasPressedThisFrame && canvas)
            canvas.SetActive(!canvas.activeSelf);
    }

    // -------------------------------------------------------------------------
    // SLIDER CALLBACKS
    // -------------------------------------------------------------------------

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

    public void setTimeStep()
    {
        if (!timeStepSlider) return;
        Static.timeStep  = timeStepSlider.value;
        Time.timeScale   = Static.timeStep;
        if (timeStepText) timeStepText.text = "time: " + Static.timeStep.ToString("F2");
    }

    // -------------------------------------------------------------------------
    // TOGGLE CALLBACKS
    // -------------------------------------------------------------------------

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
        // Show force slider OR velocity slider depending on mode
        if (ballForceSlider)    ballForceSlider.gameObject.SetActive(!Static.velocityDirectToggle);
        if (ballVelocitySlider) ballVelocitySlider.gameObject.SetActive(Static.velocityDirectToggle);
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    // Initialises a slider to a starting value and wires its onValueChanged listener.
    // The null check means you can leave any slider unassigned in the inspector
    // and this script will still compile and run without errors.
    private void InitSlider(Slider s, float startValue, UnityEngine.Events.UnityAction<float> callback)
    {
        if (!s) return;
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