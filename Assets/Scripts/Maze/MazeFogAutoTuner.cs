using UnityEngine;

public class MazeFogAutoTuner : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("The color of the void (Fog and Camera Background)")]
    [SerializeField] private Color _voidColor = Color.black;

    [Tooltip("1.0 = fog ends exactly at the wall, 0.9 = fog ends slightly before")]
    [Range(0.5f, 0.95f)]
    [SerializeField] private float _fogDensity = 0.85f;

    private void Start() => ApplyFogSettings();

    public void ApplyFogSettings()
    {
        MazeGenerator generator = Object.FindFirstObjectByType<MazeGenerator>();

        int renderDist = LocalSettings.RenderDistance;
        int chunkSize = LocalSettings.ChunkSize;
        float cellSize = 2f;

        if (generator != null)
            cellSize = (generator.CellSizeX + generator.CellSizeY + generator.CellSizeZ) / 3f;

        float maxVisibleDistance = renderDist * chunkSize * cellSize;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = _voidColor;
            mainCam.farClipPlane = maxVisibleDistance + 10f;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = _voidColor;
        RenderSettings.fogStartDistance = maxVisibleDistance * 0.25f;
        RenderSettings.fogEndDistance = maxVisibleDistance * _fogDensity;
    }
}
