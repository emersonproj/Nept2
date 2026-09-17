using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

// SETUP: Put PNG files in Assets/Resources/Images/
// Each PNG must have Read/Write enabled in its import settings (Texture Import Settings -> Advanced).

public static class ImageCreater
{
    public static BouncyShoot bouncyShootRef;

    public static List<BallClass> fireImage(Texture2D image, int divideSizeBy, GameObject imgParent)
    {
        List<BallClass> newBalls = new List<BallClass>();
        Color[] pixels = image.GetPixels(0, 0, image.width, image.height);

        // Unity GetPixels() stores pixels in row-major order, bottom-left origin.
        // Correct index for pixel at column x, row y is: y * image.width + x
        // The original code used i * image.height + j which is wrong for non-square images
        // and caused landscape images to only show the left half.

        for (int x = 0; x < image.width; x += divideSizeBy)
        {
            for (int y = 0; y < image.height; y += divideSizeBy)
            {
                Color color = pixels[y * image.width + x];

                if (color != Color.clear)
                {
                    Vector3 addedVect = new Vector3(
                        x / divideSizeBy - (image.width  / divideSizeBy) / 2f,
                        y / divideSizeBy - (image.height / divideSizeBy) / 2f,
                        0);

                    BallClass ballS = bouncyShootRef.ballFire(
                        bouncyShootRef.gameObject, addedVect, color);
                    ballS.ball.transform.parent = imgParent.transform;
                    newBalls.Add(ballS);
                }
            }
        }
        return newBalls;
    }
}