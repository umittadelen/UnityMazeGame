using UnityEngine;

public class MazeGameManager : MonoBehaviour
{
    public static MazeGameManager Instance;

    // Game Settings
    public int MazeWidth = 30;
    public int MazeHeight = 30;
    public int MazeDepth = 30;
    public int Seed = -1;

    // Performance / Chunk Settings
    public int ChunkSize = 5;       // Default value
    public int RenderDistance = 1;  // Default value

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}