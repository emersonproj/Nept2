using System.Collections.Generic;
using System.Globalization;
using DefaultNamespace;
using UnityEngine;

// Loads shape data from Assets/Resources/ShapeData/*.txt
// Each file: first line = shape name, remaining lines = "x y z" positions.
//
// SETUP:
//   1. Create the folder Assets/Resources/ShapeData/
//   2. Drop the provided .txt shape files into it
//   3. Add this script as a component on the same GameObject as BouncyShoot
//
// Keys (wired in BouncyShoot):
//   L          — fire current shape
//   Comma      — previous shape
//   Period     — next shape

public class ModelManager : MonoBehaviour
{
    public static Dictionary<string, List<Vector3>> shapeDict;
    public static string[] shapeNames;
    public static BouncyShoot bouncyShootRef;

    void Start()
    {
        LoadShapes();
    }

    void LoadShapes()
    {
        shapeDict = new Dictionary<string, List<Vector3>>();

        TextAsset[] files = Resources.LoadAll<TextAsset>("ShapeData");
        if (files.Length == 0)
        {
            Debug.LogWarning("[ModelManager] No shape files found in Resources/ShapeData/. " +
                             "Create the folder and add .txt shape files.");
            return;
        }

        foreach (TextAsset file in files)
        {
            string[] lines = file.text.Split('\n');
            if (lines.Length < 2) continue;

            string shapeName = lines[0].Trim();
            List<Vector3> points = new List<Vector3>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(' ');
                if (parts.Length < 3) continue;

                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    points.Add(new Vector3(x, y, z));
                }
            }

            if (points.Count > 0)
            {
                shapeDict[shapeName] = points;
                Debug.Log($"[ModelManager] Loaded '{shapeName}' — {points.Count} points.");
            }
        }

        shapeNames = new string[shapeDict.Count];
        shapeDict.Keys.CopyTo(shapeNames, 0);
        System.Array.Sort(shapeNames); // alphabetical so order is consistent

        Debug.Log($"[ModelManager] Loaded {shapeDict.Count} shapes: {string.Join(", ", shapeNames)}");
    }

    // Called by BouncyShoot L key
    public static List<BallClass> fireShape(GameObject parent)
    {
        List<BallClass> ballStructs = new List<BallClass>();

        if (shapeDict == null || shapeNames == null || shapeNames.Length == 0)
        {
            Debug.LogWarning("[ModelManager] No shapes loaded.");
            return ballStructs;
        }

        string name = shapeNames[Static.modelIndex % shapeNames.Length];
        List<Vector3> points = shapeDict[name];

        Debug.Log($"[ModelManager] Firing '{name}' ({points.Count} balls).");

        foreach (Vector3 point in points)
        {
            BallClass ballS = bouncyShootRef.ballFire(
                bouncyShootRef.gameObject,
                point * Static.ballSeperatness,
                Color.clear);
            ballS.ball.transform.parent = parent.transform;
            ballStructs.Add(ballS);
        }

        return ballStructs;
    }

    // Returns the name of the currently selected shape
    public static string currentShapeName()
    {
        if (shapeNames == null || shapeNames.Length == 0) return "none";
        return shapeNames[Static.modelIndex % shapeNames.Length];
    }
}