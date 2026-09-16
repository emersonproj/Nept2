using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Drop-in from original — no changes needed.
// SETUP: Attach this to a GameObject called "ExtraCams".
// Add child GameObjects to it, each with a Camera component.
// The more child cameras you have, the more camera mode variety you get.
// 128 child cameras covers all modes; fewer is fine, unused modes just show nothing extra.

public class CameraManager : MonoBehaviour
{
    public Camera[] extraCams;
    public static readonly int NUMBERCAMTYPES = 20;
    public int camSetting;

    public static readonly float INITIALZOOM = -50;
    private float currentZoom = -100;

    void Start()
    {
        extraCams = GetComponentsInChildren<Camera>();
        allReset();
    }

    public void switchCam(int camSettingGiven)
    {
        camSetting = camSettingGiven;
        allReset();

        List<Camera> cams;
        switch (camSetting)
        {
            case 0: break;
            case 1:  rotateOtherCams(.7f, 2, circleAroundCenter(2, 50)); break;
            case 2:  rotateOtherCams(.6f, 2, camerasBetweenZPoints(-150, -250, 12)); break;
            case 3:
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        int idx = i * 4 + j;
                        if (idx >= extraCams.Length) break;
                        extraCams[idx].enabled = true;
                        extraCams[idx].gameObject.transform.position = new Vector3(0, 0, currentZoom - (i * 30));
                        extraCams[idx].gameObject.transform.rotation = Quaternion.Euler(0, 0, j * 90);
                    }
                break;
            case 4:  rotateOtherCams(.1f,  4,  zRotateAroundCenter(8));  break;
            case 5:  circleAroundCenter(8,  currentZoom * 1.5f);         break;
            case 6:  circleAroundCenter(15, currentZoom * 2);            break;
            case 7:  zRotateAroundCenter(8);                             break;
            case 8:  rotateOtherCams(.7f,  8,  zRotateAroundCenter(9));  break;
            case 9:  rotateHalfCams(.5f,   9,  zRotateAroundCenter(9));  break;
            case 10: zRotateAroundCenter(11);                            break;
            case 11:
                cams = setupCamRange(0, 8);
                incrementFoV(cams, currentZoom * .7f, currentZoom * 1.3f);
                break;
            case 12: rotateOtherCams(.1f,  12, zRotateAroundCenter(11)); break;
            case 13: zRotateAroundCenter(16);                            break;
            case 14: rotateHalfCams(.5f,   14, zRotateAroundCenter(15)); break;
            case 15: rotateOtherCams(1f,   15, zRotateAroundCenter(17)); break;
            case 16: zRotateAroundCenter(32);                            break;
            case 17: zRotateAroundCenter(64);                            break;
            case 18: zRotateAroundCenter(128);                           break;
        }

        int counter = 0;
        foreach (Camera c in extraCams)
            if (c.enabled) counter++;
        Debug.Log($"[CameraManager] camSetting={camSetting}, cameras on: {counter}");
    }

    public void zoom(float zoomMult)
    {
        currentZoom *= zoomMult;
        if (camSetting != 500)
            foreach (Camera cam in extraCams)
                cam.gameObject.transform.position *= zoomMult;
    }

    public void setCurrentZoom(float zoom)
    {
        currentZoom = zoom;
    }

    public void allReset()
    {
        foreach (Camera cam in extraCams)
        {
            cam.enabled = false;
            cam.fieldOfView = 60;
            cam.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            cam.gameObject.transform.position = new Vector3(0, 0, currentZoom);
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
            extraCams[i].gameObject.transform.position =
                new Vector3(0, 0, currentZoom);
            extraCams[i].gameObject.transform.rotation =
                Quaternion.Euler(0, 0, i * (360.0f / nOfCams));
        }
        return cams;
    }

    public List<Camera> camerasBetweenZPoints(float z1, float z2, int nOfCams)
    {
        List<Camera> cams = new List<Camera>();
        float increment = (z2 - z1) / nOfCams;
        int count = Mathf.Min(nOfCams, extraCams.Length);
        for (int i = 0; i < count; i++)
        {
            cams.Add(extraCams[i]);
            extraCams[i].enabled = true;
            extraCams[i].gameObject.transform.position = new Vector3(0, 0, z1 + i * increment);
        }
        return cams;
    }

    public List<Camera> circleAroundCenter(int nOfCams, float distFromCenter)
    {
        List<Camera> cams = new List<Camera>();
        float step = 2 * Mathf.PI / nOfCams;
        int indexCounter = 0;
        for (float theta = 0; theta < 2 * Mathf.PI && indexCounter < extraCams.Length; theta += step)
        {
            float x = 0 - distFromCenter * Mathf.Cos(theta);
            float z = 0 - distFromCenter * Mathf.Sin(theta);
            cams.Add(extraCams[indexCounter]);
            extraCams[indexCounter].enabled = true;
            extraCams[indexCounter].gameObject.transform.position = new Vector3(x, 0, z);
            extraCams[indexCounter].gameObject.transform.LookAt(Vector3.zero);
            indexCounter++;
        }
        return cams;
    }

    public void incrementFoV(List<Camera> cams, float v1, float v2)
    {
        float increment = (v2 - v1) / cams.Count;
        for (int i = 0; i < cams.Count; i++)
            cams[i].fieldOfView = v1 + i * increment;
    }

    public IEnumerator camRotate(float roSpeed, int camSet, Camera cam)
    {
        float currentZRotation = 0;
        while (camSetting == camSet)
        {
            cam.gameObject.transform.rotation = Quaternion.Euler(0, 0, currentZRotation);
            currentZRotation += roSpeed;
            yield return null;
        }
    }

    public void rotateHalfCams(float speed, int caseNumber, List<Camera> cams)
    {
        for (int i = 0; i < cams.Count; i++)
            StartCoroutine(camRotate(i < cams.Count / 2 ? speed : -speed, caseNumber, cams[i]));
    }

    public void rotateOtherCams(float speed, int caseNumber, List<Camera> cams)
    {
        for (int i = 0; i < cams.Count; i++)
            StartCoroutine(camRotate(i % 2 == 0 ? speed : -speed, caseNumber, cams[i]));
    }
}