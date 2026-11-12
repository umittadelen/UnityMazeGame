using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Chromatic aberration + grain when standing under a fluorescent light. Pulses with its flicker.
public class LightAnomalyEffect : MonoBehaviour
{
    [SerializeField] private float _range = 3.5f;
    [SerializeField, Range(0f, 1f)] private float _strength = 1f;

    private Volume _volume;

    private void Awake()
    {
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.Add<ChromaticAberration>().intensity.Override(1f);
        var grain = profile.Add<FilmGrain>();
        grain.type.Override(FilmGrainLookup.Medium4);
        grain.intensity.Override(1f);

        _volume = gameObject.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = 100f;
        _volume.weight = 0f;
        _volume.sharedProfile = profile;
    }

    private void OnDestroy() => Destroy(_volume.sharedProfile);

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 eye = cam.transform.position;
        float target = 0f;

        foreach (var light in OldFluorescentLightGlowController.Active)
        {
            Vector3 toLight = light.transform.position - eye;
            float d = toLight.magnitude;

            if (toLight.y <= 0f || d >= _range) continue;
            if (Physics.Raycast(eye, toLight / d, d - 0.1f)) continue;

            target = Mathf.Max(target, (1f - d / _range) * light.Brightness);
        }

        _volume.weight = Mathf.MoveTowards(_volume.weight, target * _strength, 4f * Time.deltaTime);
    }
}