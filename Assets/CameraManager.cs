using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SETUP:
//   1. Create an empty GameObject called "ExtraCams" and add this script to it
//   2. Add 32 child GameObjects under it, each with a Camera component
//   3. On each child camera's Universal Additional Camera Data, set Render Type to Overlay
//   4. On your Main Camera's Universal Additional Camera Data, add all 32 to the Camera Stack
//   5. Leave them all disabled — allReset() handles that on startup
//
// Keys (in BouncyShoot):
//   Quote (') — cycle camera modes
//   E          — reset camera to origin

public class CameraManager : MonoBehaviour
{
    public Camera[] extraCams;
    public static readonly int NUMBERCAMTYPES = 20;
    public int camSetting;

    public static readonly float INITIALZOOM = -50;
    private float currentZoom = -100;

    // -------------------------------------------------------------------------

    void Start()
    {
        extraCams = GetComponentsInChildren<Camera>(true);
        Debug.Log($"[CameraManager] Found {extraCams.Length} child cameras.");
        allReset();
    }

    // -------------------------------------------------------------------------
    // CAMERA MODES
    // -------------------------------------------------------------------------

    public void switchCam(int camSettingGiven)
    {
        camSetting = camSettingGiven;
        allReset();

        List<Camera> cams;
        switch (camSetting)
        {
            case 0:  break;
            case 1:  rotateOtherCams(.07f,  2,  circleAroundCenter(2,  50));            break;
            case 2:  rotateOtherCams(.06f,  2,  camerasBetweenZPoints(-150, -250, 12)); break;
            case 3:
                int count3 = Mathf.Min(12, extraCams.Length);
                for (int i = 0; i < count3; i++)
                {
                    extraCams[i].enabled = true;
                    extraCams[i].transform.position = new Vector3(0, 0, currentZoom - ((i / 4) * 30));
                    extraCams[i].transform.rotation = Quaternion.Euler(0, 0, (i % 4) * 90);
                }
                break;
            case 4:  rotateOtherCams(.01f,  4,  zRotateAroundCenter(8));               break;
            case 5:  circleAroundCenter(8,  currentZoom * 1.5f);                      break;
            case 6:  circleAroundCenter(15, currentZoom * 2);                         break;
            case 7:  zRotateAroundCenter(8);                                          break;
            case 8:  rotateOtherCams(.07f,  8,  zRotateAroundCenter(9));               break;
            case 9:  rotateHalfCams(.05f,   9,  zRotateAroundCenter(9));               break;
            case 10: zRotateAroundCenter(11);                                         break;
            case 11:
                cams = setupCamRange(0, 8);
                incrementFoV(cams, currentZoom * .7f, currentZoom * 1.3f);
                break;
            case 12: rotateOtherCams(.01f,  12, zRotateAroundCenter(11));              break;
            case 13: zRotateAroundCenter(16);                                         break;
            case 14: rotateHalfCams(.05f,   14, zRotateAroundCenter(15));              break;
            case 15: rotateOtherCams(.1f,   15, zRotateAroundCenter(17));              break;
            case 16: zRotateAroundCenter(Mathf.Min(32, extraCams.Length));            break;
            case 17: zRotateAroundCenter(Mathf.Min(32, extraCams.Length));            break;
            case 18: zRotateAroundCenter(Mathf.Min(32, extraCams.Length));            break;
        }

        int on = 0;
        foreach (Camera c in extraCams) if (c.enabled) on++;
        Debug.Log($"[CameraManager] Mode {camSetting} — {on} cameras active.");
    }

    public void zoom(float zoomMult)
    {
        currentZoom *= zoomMult;
        if (camSetting != 500)
            foreach (Camera cam in extraCams)
                cam.transform.position *= zoomMult;
    }

    public void setCurrentZoom(float zoom)
    {
        currentZoom = zoom;
    }

    // -------------------------------------------------------------------------
    // UTILITIES
    // -------------------------------------------------------------------------

    public void allReset()
    {
        foreach (Camera cam in extraCams)
        {
            cam.enabled = false;
            cam.fieldOfView = 60;
            cam.transform.rotation = Quaternion.Euler(Vector3.zero);
            cam.transform.position  = new Vector3(0, 0, currentZoom);
            cam.nearClipPlane = .3f;
            cam.farClipPlane  = 20000;
        }
    }

    public List<Camera> setupCamRange(int index1, int index2)
    {
        List<Camera> cams = new List<Camera>();
        for (int i = index1; i <= Mathf.Min(index2, extraCams.Length - 1); i++)
        {
            cams.Add(extraCams[i]);
            extraCams[i].enabled = true;
        }
        return cams;
    }

    public List<Camera> zRotateAroundCenter(int nOfCams)
    {
        List<Camera> cams = new List<Camera>();
        int count = Mathf.Min(nOfCams, extraCams.Length);
        for (int i = 0; i < count; i++)
        {
            cams.Add(extraCams[i]);
            extraCams[i].enabled = true;
            extraCams[i].transform.position = new Vector3(0, 0, currentZoom);
            extraCams[i].transform.rotation = Quaternion.Euler(0, 0, i * (360f / nOfCams));
        }
        return cams;
    }

    public List<Camera> camerasBetweenZPoints(float z1, float z2, int nOfCams)
    {
        List<Camera> cams = new List<Camera>();
        float inc = (z2 - z1) / nOfCams;
        int count = Mathf.Min(nOfCams, extraCams.Length);
        for (int i = 0; i < count; i++)
        {
            cams.Add(extraCams[i]);
            extraCams[i].enabled = true;
            extraCams[i].transform.position = new Vector3(0, 0, z1 + i * inc);
        }
        return cams;
    }

    public List<Camera> circleAroundCenter(int nOfCams, float distFromCenter)
    {
        List<Camera> cams = new List<Camera>();
        int count = Mathf.Min(nOfCams, extraCams.Length);
        float step = count > 0 ? 2 * Mathf.PI / count : 0;
        for (int i = 0; i < count; i++)
        {
            float theta = i * step;
            extraCams[i].enabled = true;
            extraCams[i].transform.position = new Vector3(
                -distFromCenter * Mathf.Cos(theta), 0,
                -distFromCenter * Mathf.Sin(theta));
            extraCams[i].transform.LookAt(Vector3.zero);
            cams.Add(extraCams[i]);
        }
        return cams;
    }

    public void incrementFoV(List<Camera> cams, float v1, float v2)
    {
        float inc = cams.Count > 0 ? (v2 - v1) / cams.Count : 0;
        for (int i = 0; i < cams.Count; i++)
            cams[i].fieldOfView = v1 + i * inc;
    }

    public IEnumerator camRotate(float roSpeed, int camSet, Camera cam)
    {
        float rot = 0;
        while (camSetting == camSet)
        {
            cam.transform.rotation = Quaternion.Euler(0, 0, rot);
            rot += roSpeed;
            yield return null;
        }
    }

    public void rotateHalfCams(float speed, int caseNum, List<Camera> cams)
    {
        for (int i = 0; i < cams.Count; i++)
            StartCoroutine(camRotate(i < cams.Count / 2 ? speed : -speed, caseNum, cams[i]));
    }

    public void rotateOtherCams(float speed, int caseNum, List<Camera> cams)
    {
        for (int i = 0; i < cams.Count; i++)
            StartCoroutine(camRotate(i % 2 == 0 ? speed : -speed, caseNum, cams[i]));
    }
}