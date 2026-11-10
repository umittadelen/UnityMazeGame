using System;
using UnityEngine;

// Everything that must match on every machine to produce the same maze.
[Serializable]
public struct MatchConfig
{
    public int Width;
    public int Height;
    public int Depth;
    public int Seed;        // always resolved, never -1
    public bool RandomSeed; // Play Again rerolls when true

    public static MatchConfig Create(int width, int height, int depth, int seed)
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        depth = Mathf.Max(1, depth);
        if (width == 1 && height == 1) width = 2;
        if (depth == 1 && (width == 1 || height == 1)) depth = 2;

        bool random = seed == -1;
        return new MatchConfig
        {
            Width = width, Height = height, Depth = depth,
            Seed = random ? NewSeed() : seed,
            RandomSeed = random
        };
    }

    public static MatchConfig FromInput(string width, string height, string depth, string seedText)
    {
        int.TryParse(width, out int w);
        int.TryParse(height, out int h);
        int.TryParse(depth, out int d);
        return Create(w, h, d, ParseSeed(seedText));
    }

    public MatchConfig Rerolled()
    {
        var c = this;
        if (c.RandomSeed) c.Seed = NewSeed();
        return c;
    }

    // Empty -> random, number -> number, text -> hash that is the same on every machine.
    private static int ParseSeed(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return -1;
        text = text.Trim();
        if (int.TryParse(text, out int n)) return n;

        unchecked
        {
            uint h = 2166136261;
            foreach (char ch in text) { h ^= ch; h *= 16777619; }
            return (int)(h & 0x7FFFFFFF);
        }
    }

    // Not UnityEngine.Random: maze generation reseeds it, so it would repeat.
    public static int NewSeed() => (int)((DateTime.UtcNow.Ticks ^ Guid.NewGuid().GetHashCode()) & 0x7FFFFFFF);
}
