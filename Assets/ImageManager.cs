using UnityEngine;

// Loads all Texture2D assets from Assets/Resources/Images/ at startup.
// SETUP:
//   1. Create the folder Assets/Resources/Images/
//   2. Drop PNG files into it
//   3. For each PNG, select it in the Project window, tick Read/Write
//      under Texture Import Settings -> Advanced, and hit Apply
//   4. Add this script as a component on the same GameObject as BouncyShoot
//
// Keys (wired in BouncyShoot):
//   [ (left bracket)  — previous image
//   ] (right bracket) — next image
//   I                 — fire current image

public class ImageManager : MonoBehaviour
{
    public static Texture2D[] images;
    public static string[]    imageNames;
    public static int         currentIndex = 0;

    void Start()
    {
        images = Resources.LoadAll<Texture2D>("Images");

        if (images.Length == 0)
        {
            Debug.LogWarning("[ImageManager] No images found in Resources/Images/. " +
                             "Add PNGs there and enable Read/Write in their import settings.");
            return;
        }

        imageNames = new string[images.Length];
        for (int i = 0; i < images.Length; i++)
            imageNames[i] = images[i].name;

        // Set the first image as current so I key works immediately
        Static.currentImage = images[0];

        Debug.Log($"[ImageManager] Loaded {images.Length} images: {string.Join(", ", imageNames)}");
    }

    public static void cycleNext()
    {
        if (images == null || images.Length == 0) return;
        currentIndex = (currentIndex + 1) % images.Length;
        Static.currentImage = images[currentIndex];
        Debug.Log($"[ImageManager] Image: {currentImageName()} ({currentIndex + 1}/{images.Length})");
    }

    public static void cyclePrev()
    {
        if (images == null || images.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = images.Length - 1;
        Static.currentImage = images[currentIndex];
        Debug.Log($"[ImageManager] Image: {currentImageName()} ({currentIndex + 1}/{images.Length})");
    }

    public static string currentImageName()
    {
        if (images == null || images.Length == 0) return "none";
        return imageNames[currentIndex];
    }
}