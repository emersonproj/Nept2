using System.Collections.Generic;
using UnityEngine;

// Drop-in from original.
// SETUP: Create a folder at Assets/Resources/VertModels and place prefabs in it.
// Each prefab needs a MeshFilter component — the script reads its vertices.
// The D key fires whichever model is at Static.vertModelIndex.

namespace DefaultNamespace
{
    public class MeshVertManager : MonoBehaviour
    {
        public Object[] vertModels;
        public static Dictionary<int, VertsObject> vertModelDict;
        public static BouncyShoot bouncyShootRef;

        void Start()
        {
            vertModelDict = new Dictionary<int, VertsObject>();
            vertModels = Resources.LoadAll("VertModels", typeof(GameObject));
            Debug.Log($"[MeshVertManager] Loaded {vertModels.Length} vert models.");

            int i = 0;
            foreach (Object o in vertModels)
            {
                GameObject g = (GameObject)o;
                vertModelDict[i] = new VertsObject(
                    g.name,
                    g.GetComponent<MeshFilter>().sharedMesh.vertices,
                    g.transform.localScale.x);
                i++;
            }
        }

        public static List<BallClass> fireVertModel(GameObject modelParent)
        {
            List<BallClass> ballStructs = new List<BallClass>();

            if (vertModelDict == null || vertModelDict.Count == 0)
            {
                Debug.LogWarning("[MeshVertManager] No vert models loaded. Add prefabs to Assets/Resources/VertModels.");
                return ballStructs;
            }

            VertsObject vertObj = vertModelDict[Static.vertModelIndex % vertModelDict.Count];

            for (int i = 0; i < vertObj.verts.Length; i++)
            {
                if (i % Static.imageDivideBy == 0)
                {
                    BallClass ballS = bouncyShootRef.ballFire(bouncyShootRef.gameObject,
                        vertObj.verts[i] * vertObj.scale * Static.ballSeperatness, Color.clear);
                    ballS.ball.transform.parent = modelParent.transform;
                    ballStructs.Add(ballS);
                }
            }
            return ballStructs;
        }
    }

    public class VertsObject
    {
        public VertsObject(string n, Vector3[] vs, float s = 1)
        {
            name  = n;
            scale = s;
            verts = vs;
        }
        public readonly string   name;
        public readonly float    scale;
        public readonly Vector3[] verts;
    }
}