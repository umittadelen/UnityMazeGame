using UnityEngine;
using UnityEngine.InputSystem;

public class WarningScreen : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MenuScene";

    private InputAction anyKeyAction;

    private void Awake()
    {
        anyKeyAction = new InputAction(type: InputActionType.Button, binding: "*/<button>");
        anyKeyAction.Enable();
    }

    private void OnDestroy()
    {
        anyKeyAction.Dispose();
    }

    private void Update()
    {
        if (anyKeyAction.WasPressedThisFrame()) Continue();
    }

    public void Continue() => SceneLoader.Load(menuSceneName);
}
