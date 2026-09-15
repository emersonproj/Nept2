using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

// Drop-in from original — no changes needed.
// Static.currentImage must be assigned (e.g. drag a Texture2D in via inspector or a loader script).
// Static.imageDivideBy controls resolution (default 4 — every 4th pixel).

public static class ImageCreater
{
    public static BouncyShoot bouncyShootRef;

    public static List<BallClass> fireImage(Texture2D image, int divideSizeBy, GameObject imgParent)
    {
        List<BallClass> newBalls = new List<BallClass>();
        Color[] pixels = image.GetPixels(0, 0, image.width, image.height);

        for (int i = 0; i < image.width; i += divideSizeBy)
        {
            for (int j = 0; j < image.height; j += divideSizeBy)
            {
                Color color = pixels[(i * image.height) + j];
                if (color != Color.clear)
                {
                    Vector3 addedVect = new Vector3(
                        j / divideSizeBy - (image.width  / divideSizeBy) / 2,
                        i / divideSizeBy - (image.height / divideSizeBy) / 2, 0);

                    BallClass ballS = bouncyShootRef.ballFire(bouncyShootRef.gameObject, addedVect, color);
                    ballS.ball.transform.parent = imgParent.transform;
                    newBalls.Add(ballS);
                }
            }
        }
        return newBalls;
    }
}