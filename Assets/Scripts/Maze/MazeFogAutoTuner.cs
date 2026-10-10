using UnityEngine;

public class MazeFogAutoTuner : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("The color of the void (Fog and Camera Background)")]
    [SerializeField] private Color _voidColor = Color.black;

    [Tooltip("How much buffer to keep? (1.0 = Fog ends exactly at the wall, 0.9 = Fog ends slightly before)")]
    [Range(0.5f, 0.95f)]
    [SerializeField] private float _fogDensity = 0.85f; 

    void Start()
    {
        ApplyFogSettings();
    }

    public void ApplyFogSettings()
    {
        // 1. Get References
        MazeGenerator generator = Object.FindFirstObjectByType<MazeGenerator>();
        
        // We try to get settings from GameManager first (User preferences), 
        // otherwise fallback to hardcoded defaults or defaults in the ChunkManager.
        int renderDist = 1;
        int chunkSize = 5;
        float cellSize = 2f; // Average cell size for fog calculation

        if (MazeGameManager.Instance != null)
        {
            renderDist = MazeGameManager.Instance.RenderDistance;
            chunkSize = MazeGameManager.Instance.ChunkSize;
        }
        
        if (generator != null)
        {
            // Use average of X, Y, Z cell sizes for fog calculation
            cellSize = (generator.CellSizeX + generator.CellSizeY + generator.CellSizeZ) / 3f;
        }

        // 2. Calculate the "Edge of the World"
        // Math: RenderDistance is the radius of chunks loaded.
        // Example: Dist 1 = 1 chunk radius. 
        // Distance = 1 * 5 cells * 2 units = 10 units away.
        float maxVisibleDistance = renderDist * chunkSize * cellSize;

        // 3. Configure Camera (The Void)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = _voidColor;
            mainCam.farClipPlane = maxVisibleDistance + 10f; // Clip just after the fog
        }

        // 4. Configure Fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = _voidColor;

        // Start fading at 25% of the distance
        RenderSettings.fogStartDistance = maxVisibleDistance * 0.25f;
        
        // Be completely solid by 85% (or _fogDensity) of the distance
        // This ensures the void is hidden before the geometry actually stops
        RenderSettings.fogEndDistance = maxVisibleDistance * _fogDensity;

        Debug.Log($"[Fog Tuner] View Limit: {maxVisibleDistance}m | Fog: {RenderSettings.fogStartDistance}m - {RenderSettings.fogEndDistance}m");
    }
}