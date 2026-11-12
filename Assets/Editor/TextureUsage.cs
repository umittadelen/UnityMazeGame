using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class TextureUsage
{
    [MenuItem("Assets/Find Texture Usage", true)]
    private static bool Validate() => Selection.activeObject is Texture;

    [MenuItem("Assets/Find Texture Usage")]
    private static void Find()
    {
        var tex = (Texture)Selection.activeObject;
        string texPath = AssetDatabase.GetAssetPath(tex);

        string[] users = AssetDatabase.FindAssets("t:Material t:Prefab t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => AssetDatabase.GetDependencies(p, true).Contains(texPath))
            .ToArray();

        var mats = users.Where(p => p.EndsWith(".mat"))
            .Select(AssetDatabase.LoadAssetAtPath<Material>).ToArray();

        int inScene = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                          .Count(r => r.sharedMaterials.Any(m => mats.Contains(m)))
                    + Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                          .Count(i => i.sprite != null && i.sprite.texture == tex)
                    + Object.FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                          .Count(i => i.texture == tex);

        Debug.Log($"[{tex.name}] objects in open scene: {inScene}\nAssets using it ({users.Length}):\n" + string.Join("\n", users));
    }
}
