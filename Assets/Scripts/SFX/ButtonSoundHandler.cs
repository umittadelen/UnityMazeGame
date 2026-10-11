using UnityEngine;

public class ButtonSoundHandler : MonoBehaviour
{
    [SerializeField] private AudioClip _hoverSound;
    [SerializeField] private AudioClip _clickSound;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.6f;
    private AudioSource _audioSource;
    [SerializeField] private UnityEngine.UI.Button[] _buttons;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        
        if (_audioSource == null)
        {
            Debug.LogError("AudioSource component not found on ButtonSoundHandler!");
            return;
        }

        if (_buttons == null || _buttons.Length == 0)
        {
            Debug.LogWarning("No buttons assigned to ButtonSoundHandler!");
            return;
        }

        foreach (var button in _buttons)
        {
            if (button == null) continue;

            UnityEngine.EventSystems.EventTrigger trigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // Add pointer enter event (hover)
            var pointerEnterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener((_) => PlayHoverSound());
            trigger.triggers.Add(pointerEnterEntry);

            // Add pointer click event
            button.onClick.AddListener(PlayClickSound);
        }
    }

    void Update()
    {
        
    }

    private void PlayHoverSound()
    {
        if (_hoverSound != null)
        {
            _audioSource.PlayOneShot(_hoverSound, _sfxVolume);
        }
    }

    private void PlayClickSound()
    {
        if (_clickSound != null)
        {
            _audioSource.PlayOneShot(_clickSound, _sfxVolume);
        }
    }
}
