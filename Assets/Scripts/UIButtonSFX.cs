
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSFX : MonoBehaviour,
    IPointerEnterHandler, IPointerClickHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
            UIAudioManager.Instance?.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && button.interactable &&
            eventData.button == PointerEventData.InputButton.Left)
        {
            UIAudioManager.Instance?.PlayClick();
        }
    }
}
