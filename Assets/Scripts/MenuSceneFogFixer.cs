using UnityEngine;

public class MenuSceneFogFixer : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogStartDistance = 1.25f;
        RenderSettings.fogEndDistance = 4.5f;

        if (targetCamera != null)
        {
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = Color.black;
            targetCamera.farClipPlane = 10f;
        }
    }
}