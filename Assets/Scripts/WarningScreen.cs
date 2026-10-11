
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarningScreen : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MenuScene";

    private bool switching;

    private void Start()
    {
    }

    private void Update()
    {
        if (!switching && Input.anyKeyDown)
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
