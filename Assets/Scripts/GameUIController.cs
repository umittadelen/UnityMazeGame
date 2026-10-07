using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections; // Required for Coroutines

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _pausePanelDialog;
    [SerializeField] private GameObject _endGamePanelDialog;
    [SerializeField] private TextMeshProUGUI _seedDisplayText;

    private bool _isPaused = false;
    private int _currentSeed;
    private bool _gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (_pausePanelDialog) _pausePanelDialog.SetActive(false);
        LockCursor();

        // Get the seed immediately
        UpdateSeedDisplay();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void UpdateSeedDisplay()
    {
        // Find the generator to get the REAL seed
        MazeGenerator generator = Object.FindFirstObjectByType<MazeGenerator>();
        
        if (generator != null)
        {
            _currentSeed = generator.GetUsedSeed();
            if (_seedDisplayText != null)
                _seedDisplayText.text = $"Seed: {_currentSeed}";
        }
    }

    // --- NEW: COPY FUNCTION ---
    public void CopySeedToClipboard()
    {
        // 1. Copy to System Clipboard
        GUIUtility.systemCopyBuffer = _currentSeed.ToString();
        
        // 2. Show Visual Feedback
        if (_seedDisplayText != null)
        {
            StartCoroutine(ShowCopiedFeedback());
        }
        
        Debug.Log($"Copied seed to clipboard: {_currentSeed}");
    }

    private IEnumerator ShowCopiedFeedback()
    {
        // Change text to "Copied!"
        _seedDisplayText.text = "Copied!";
        
        // Wait for 1 second (ignoring time scale so it works while paused!)
        yield return new WaitForSecondsRealtime(1f);
        
        // Change back to original text
        _seedDisplayText.text = $"Seed: {_currentSeed}";
    }

    // --- STANDARD BUTTONS ---

    public void ResumeGame()
    {
        _isPaused = false;
        _pausePanelDialog.SetActive(false);
        Time.timeScale = 1f;
        LockCursor();
    }

    public void PauseGame()
    {
        _isPaused = true;
        _pausePanelDialog.SetActive(true);
        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene"); 
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- 3. NEW: GAME WON LOGIC ---
    public void GameWon()
    {
        if (_gameEnded) return; // Don't trigger twice
        _gameEnded = true;

        Debug.Log("Game Won! Showing UI.");

        if (_endGamePanelDialog != null)
        {
            _endGamePanelDialog.SetActive(true);
            
            // Stop the game
            Time.timeScale = 0f;
            
            // Show Mouse
            UnlockCursor();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}