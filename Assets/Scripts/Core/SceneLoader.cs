using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Creates itself on startup. Fade to black -> load -> fade back in.
public class SceneLoader : MonoBehaviour
{
    private const float FadeOutTime = 0.35f;
    private const float FadeInTime = 0.6f;

    public static bool IsLoading { get; private set; }

    private static SceneLoader _instance;
    private CanvasGroup _group;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        IsLoading = false;
        var go = new GameObject("[SceneLoader]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<SceneLoader>();
        _instance.BuildOverlay();
    }

    public static void Load(string sceneName)
    {
        if (IsLoading) return;
        _instance.StartCoroutine(_instance.LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        _group.blocksRaycasts = true;

        yield return Fade(1f, FadeOutTime);
        Time.timeScale = 1f;
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null; // maze is built in Start, keep that frame hidden
        yield return Fade(0f, FadeInTime);

        _group.blocksRaycasts = false;
        IsLoading = false;
    }

    private IEnumerator Fade(float to, float duration)
    {
        float from = _group.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetAlpha(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        _group.alpha = a;
        AudioListener.volume = 1f - a;
    }

    private void BuildOverlay()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        gameObject.AddComponent<GraphicRaycaster>();

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;

        var fade = new GameObject("Fade", typeof(RectTransform), typeof(Image));
        fade.transform.SetParent(transform, false);
        var rt = (RectTransform)fade.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        fade.GetComponent<Image>().color = Color.black;
    }
}
