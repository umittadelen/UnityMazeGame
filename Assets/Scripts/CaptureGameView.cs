
using UnityEngine;
using System.IO;

public class CaptureGameView : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
            Capture();
    }

    void Capture()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        int width = 1920;
        int height = 1080;

        RenderTexture rt = new RenderTexture(
            width, height, 24, RenderTextureFormat.ARGB32);

        RenderTexture previous = cam.targetTexture;
        RenderTexture previousActive = RenderTexture.active;

        Texture2D image = new Texture2D(
            width, height, TextureFormat.RGB24, false);

        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        cam.targetTexture = previous;
        RenderTexture.active = previousActive;

        string path = Path.Combine(
            Application.persistentDataPath, "MenuBackground.png");

        File.WriteAllBytes(path, image.EncodeToPNG());

        Destroy(image);
        rt.Release();
        Destroy(rt);

        Debug.Log("Screenshot saved to: " + path);
    }
}
