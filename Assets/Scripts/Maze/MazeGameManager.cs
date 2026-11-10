using UnityEngine;

// Test settings, used only when GameScene is played directly in the editor.
public class MazeGameManager : MonoBehaviour
{
    public int MazeWidth = 30;
    public int MazeHeight = 30;
    public int MazeDepth = 30;
    public int Seed = -1;

    private void Awake()
    {
        if (!GameSession.HasConfig)
            GameSession.Begin(MatchConfig.Create(MazeWidth, MazeHeight, MazeDepth, Seed));
    }
}
