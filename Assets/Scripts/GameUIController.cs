using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _pausePanelDialog;
    [SerializeField] private GameObject _endGamePanelDialog;
    [SerializeField] private TextMeshProUGUI _seedDisplayText;

    [SerializeField] private AudioClip _winSound;

    private bool _isPaused;
    private bool _gameEnded;

    private int Seed => GameSession.Config.Seed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (_pausePanelDialog) _pausePanelDialog.SetActive(false);
        if (_endGamePanelDialog) _endGamePanelDialog.SetActive(false);
        SetCursorLocked(true);
        if (_seedDisplayText != null) _seedDisplayText.text = $"Seed: {Seed}";
    }

    private void Update()
    {
        if (_gameEnded || SceneLoader.IsLoading || Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void CopySeedToClipboard()
    {
        GUIUtility.systemCopyBuffer = Seed.ToString();
        if (_seedDisplayText != null) StartCoroutine(ShowCopiedFeedback());
    }

    private IEnumerator ShowCopiedFeedback()
    {
        _seedDisplayText.text = "Copied!";
        yield return new WaitForSecondsRealtime(1f);
        _seedDisplayText.text = $"Seed: {Seed}";
    }

    public void ResumeGame()
    {
        _isPaused = false;
        _pausePanelDialog.SetActive(false);
        Time.timeScale = 1f;
        SetCursorLocked(true);
    }

    public void PauseGame()
    {
        _isPaused = true;
        _pausePanelDialog.SetActive(true);
        Time.timeScale = 0f;
        SetCursorLocked(false);
    }

    public void ExitToMainMenu() => SceneLoader.Load("MenuScene");

    public void RestartGame()
    {
        if (SceneLoader.IsLoading) return;
        GameSession.Reroll();
        SceneLoader.Load(SceneManager.GetActiveScene().name);
    }

    public void GameWon()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (_winSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(_winSound, Camera.main.transform.position);

        if (_endGamePanelDialog != null)
        {
            _endGamePanelDialog.SetActive(true);
            Time.timeScale = 0f;
            SetCursorLocked(false);
        }
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
