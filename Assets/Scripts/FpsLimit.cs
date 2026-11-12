using UnityEngine;
using TMPro;

public class FpsLimit : MonoBehaviour
{
    public TMP_Text label;

    int[] fpsSteps = { 30, 60, 120, 144, 240, -1 };
    int index = 1;

    void Start()
    {
        index = PlayerPrefs.GetInt("fps", 1);
        if (index < 0 || index >= fpsSteps.Length) index = 1;
        UpdateFps();
    }

    public void Next()
    {
        index++;
        if (index >= fpsSteps.Length) index = 0;
        UpdateFps();
    }

    public void Previous()
    {
        index--;
        if (index < 0) index = fpsSteps.Length - 1;
        UpdateFps();
    }

    void UpdateFps()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fpsSteps[index];
        PlayerPrefs.SetInt("fps", index);

        if (label != null)
        {
            if (fpsSteps[index] == -1) label.text = "Inf";
            else label.text = fpsSteps[index].ToString();
        }
    }
}