using UnityEngine;

// Per-machine settings (never synced in multiplayer).
public static class LocalSettings
{
    public const int DefaultChunkSize = 5;
    public const int DefaultRenderDistance = 1;

    public static int ChunkSize
    {
        get => Mathf.Max(1, PlayerPrefs.GetInt("ChunkSize", DefaultChunkSize));
        set { PlayerPrefs.SetInt("ChunkSize", Mathf.Max(1, value)); PlayerPrefs.Save(); }
    }

    public static int RenderDistance
    {
        get => Mathf.Max(1, PlayerPrefs.GetInt("RenderDistance", DefaultRenderDistance));
        set { PlayerPrefs.SetInt("RenderDistance", Mathf.Max(1, value)); PlayerPrefs.Save(); }
    }
}
