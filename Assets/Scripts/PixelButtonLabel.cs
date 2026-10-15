using UnityEngine;
using UnityEngine.UI;

public class PixelButtonLabel : MonoBehaviour
{
    [SerializeField] Image background;       // the button's Image
    [SerializeField] Button button;          // the same object's Button
    [SerializeField] RectTransform label;    // the TMP text
    [SerializeField] float idleOffset = 2.5f;     // sprite px, up
    [SerializeField] float pressedOffset = -1.5f; // sprite px, down

    bool? wasPressed;

    float PixelUnits
    {
        get
        {
            var canvas = background.canvas;
            float refPPU = canvas ? canvas.referencePixelsPerUnit : 100f;
            float spritePPU = background.sprite ? background.sprite.pixelsPerUnit : 100f;
            return refPPU / (spritePPU * background.pixelsPerUnitMultiplier);
        }
    }

    void OnEnable() => wasPressed = null;

    void LateUpdate()
    {
        // Sprite Swap sets overrideSprite, so compare that to the pressed sprite
        var shown = background.overrideSprite;
        bool pressed = shown != null && shown == button.spriteState.pressedSprite;

        if (wasPressed == pressed) return;
        wasPressed = pressed;

        float px = pressed ? pressedOffset : idleOffset;
        label.anchoredPosition = new Vector2(0, Mathf.Round(px * PixelUnits));
    }
}