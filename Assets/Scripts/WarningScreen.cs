
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WarningScreen : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MenuScene";

    private bool switching;
    private InputAction anyKeyAction;

    private void Awake()
    {
        anyKeyAction = new InputAction(type: InputActionType.Button, binding: "*/<button>");
        anyKeyAction.Enable();
    }

    private void OnDestroy()
    {
        anyKeyAction.Disable();
        anyKeyAction.Dispose();
    }

    private void Update()
    {
        if (!switching && anyKeyAction.WasPressedThisFrame())
            Continue();
    }

    public void Continue()
    {
        if (switching || string.IsNullOrEmpty(menuSceneName))
            return;

        switching = true;
        SceneManager.LoadScene(menuSceneName);
    }
}
