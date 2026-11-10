// Current match settings. Static, so it survives scene loads without a DontDestroyOnLoad object.
public static class GameSession
{
    public static MatchConfig Config { get; private set; }
    public static bool HasConfig { get; private set; }

    public static void Begin(MatchConfig config)
    {
        Config = config;
        HasConfig = true;
    }

    public static void Reroll()
    {
        if (HasConfig) Config = Config.Rerolled();
    }

    public static void End()
    {
        Config = default;
        HasConfig = false;
    }
}
