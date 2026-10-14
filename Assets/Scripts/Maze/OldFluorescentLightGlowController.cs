using System.Collections;
using UnityEngine;

public class OldFluorescentLightGlowController : MonoBehaviour
{
    [System.Serializable]
    public class Tube
    {
        public Renderer renderer;
        [Tooltip("Material slot index if the tube uses a specific slot")]
        public int materialIndex = 0;
        [Tooltip("Optional: a Light that should flicker with this tube")]
        public Light tubeLight;

        [HideInInspector] public Color baseEmission;
        [HideInInspector] public float baseLight;
        [HideInInspector] public float level = 1f;   // 0..1 brightness
        [HideInInspector] public bool dead;
        [HideInInspector] public float seed;
        [HideInInspector] public MaterialPropertyBlock block;
    }

    public Tube[] tubes;

    [Header("Timing")]
    public Vector2 calmTime = new Vector2(2f, 14f);   // seconds between events

    [Header("Chances (per event)")]
    [Range(0, 1)] public float burnoutChance = 0.01f; // very low
    [Range(0, 1)] public float hardOffChance = 0.15f;
    [Range(0, 1)] public float dipChance = 0.30f;     // remainder = flicker burst

    [Header("Look")]
    [Range(0, 0.3f)] public float humAmount = 0.04f;  // subtle constant instability

    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        foreach (var t in tubes)
        {
            if (t.renderer == null) continue;
            var mats = t.renderer.sharedMaterials;
            t.baseEmission = mats[Mathf.Min(t.materialIndex, mats.Length - 1)].GetColor(EmissionID);
            t.baseLight = t.tubeLight ? t.tubeLight.intensity : 0f;
            t.block = new MaterialPropertyBlock();
            t.seed = Random.value * 100f;
        }
    }

    void OnEnable()
    {
        foreach (var t in tubes)
        {
            if (t.renderer == null) continue;
            t.dead = false;
            t.level = 1f;
            StartCoroutine(TubeRoutine(t));
        }
    }

    void Update()
    {
        foreach (var t in tubes)
        {
            if (t.renderer == null) continue;

            float v = t.dead ? 0f :
                t.level * (1f - humAmount * Mathf.PerlinNoise(Time.time * 25f, t.seed));

            t.renderer.GetPropertyBlock(t.block, t.materialIndex);
            t.block.SetColor(EmissionID, t.baseEmission * v);
            t.renderer.SetPropertyBlock(t.block, t.materialIndex);

            if (t.tubeLight) t.tubeLight.intensity = t.baseLight * v;
        }
    }

    IEnumerator TubeRoutine(Tube t)
    {
        yield return new WaitForSeconds(Random.Range(0f, calmTime.y)); // desync copies

        while (!t.dead)
        {
            t.level = 1f;
            yield return new WaitForSeconds(Random.Range(calmTime.x, calmTime.y));

            if (Random.value < burnoutChance)
            {
                yield return Flicker(t, Random.Range(6, 16));   // dramatic last gasp
                t.dead = true;
                yield break;
            }

            float r = Random.value;
            if (r < hardOffChance)       yield return HardOff(t);
            else if (r < hardOffChance + dipChance) yield return Dip(t);
            else                         yield return Flicker(t, Random.Range(3, 10));
        }
    }

    IEnumerator Flicker(Tube t, int count)
    {
        for (int i = 0; i < count; i++)
        {
            t.level = Random.value < 0.6f ? 0f : Random.Range(0.1f, 0.5f);
            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            t.level = Random.Range(0.8f, 1f);
            yield return new WaitForSeconds(Random.Range(0.02f, 0.12f));
        }
    }

    IEnumerator Dip(Tube t)
    {
        float target = Random.Range(0.25f, 0.6f);
        float dur = Random.Range(0.1f, 0.5f);
        t.level = target;
        yield return new WaitForSeconds(dur);
        t.level = 1f;
    }

    IEnumerator HardOff(Tube t)
    {
        t.level = 0f;
        yield return new WaitForSeconds(Random.Range(0.3f, 1.5f));
        // fluorescent-style startup: stutter, then warm up
        yield return Flicker(t, Random.Range(2, 6));
        float a = 0f;
        float dur = Random.Range(0.3f, 0.8f);
        while (a < 1f)
        {
            a += Time.deltaTime / dur;
            t.level = Mathf.Lerp(0.3f, 1f, a);
            yield return null;
        }
    }
}