using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Using TextMeshPro

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuContainer;
    [SerializeField] private GameObject _newGameDialog;
    [SerializeField] private GameObject _settingsDialog;
    [SerializeField] private GameObject _creditsDialog;

    [Header("New Game Inputs")]
    [SerializeField] private TMP_InputField _mazeXInput;
    [SerializeField] private TMP_InputField _mazeYInput;
    [SerializeField] private TMP_InputField _mazeZInput;
    [SerializeField] private TMP_InputField _seedInput;

    [Header("Settings Inputs")]
    // Drag your Settings Panel inputs here
    [SerializeField] private TMP_InputField _chunkSizeInput; 
    [SerializeField] private TMP_InputField _renderDistInput;

    private void Start()
    {
        // Ensure only Main Menu is visible at start
        _mainMenuContainer.SetActive(true);
        if (_newGameDialog) _newGameDialog.SetActive(false);
        if (_settingsDialog) _settingsDialog.SetActive(false);

        // Ensure Manager exists so we have defaults
        if (MazeGameManager.Instance == null)
        {
            GameObject go = new GameObject("MazeGameManager");
            go.AddComponent<MazeGameManager>();
        }
    }

    // --- NEW GAME LOGIC ---

    public void OpenNewGamePanel()
    {
        _mainMenuContainer.SetActive(false);
        _newGameDialog.SetActive(true);
    }

    public void CloseNewGamePanel()
    {
        _newGameDialog.SetActive(false);
        _mainMenuContainer.SetActive(true);
    }

    public void StartGame()
    {
        // 1. Parse Inputs (Default to 0 if invalid/empty)
        int.TryParse(_mazeXInput.text, out int w);
        int.TryParse(_mazeYInput.text, out int h);
        int.TryParse(_mazeZInput.text, out int d);
        int.TryParse(_seedInput.text, out int seed);

        // 2. Clamp to minimum 1
        w = Mathf.Max(1, w);
        h = Mathf.Max(1, h);
        d = Mathf.Max(1, d);

        // 3. Validation Logic
        // If Width and Height are both 1, force Width to 2
        if (w == 1 && h == 1) w = 2;
        // If Depth is 1 and we still have another 1 (either W or H), force Depth to 2
        if (d == 1 && (w == 1 || h == 1)) d = 2;

        // 4. Save and Load
        if (MazeGameManager.Instance == null)
        {
            GameObject go = new GameObject("MazeGameManager");
            go.AddComponent<MazeGameManager>();
        }

        MazeGameManager.Instance.MazeWidth = w;
        MazeGameManager.Instance.MazeHeight = h;
        MazeGameManager.Instance.MazeDepth = d;

        // Handle Seed logic (Random if -1 or empty)
        if (string.IsNullOrEmpty(_seedInput.text)) seed = -1;
        else if (seed == 0 && _seedInput.text != "0") seed = _seedInput.text.GetHashCode();
        
        MazeGameManager.Instance.Seed = seed;

        MazeGameManager.Instance.ChunkSize = SettingsManager.LoadInt("ChunkSize", MazeGameManager.Instance.ChunkSize);
        MazeGameManager.Instance.RenderDistance = SettingsManager.LoadInt("RenderDistance", MazeGameManager.Instance.RenderDistance);

        SceneManager.LoadScene("GameScene");
    }

    // --- SETTINGS LOGIC (Save & Cancel) ---

    public void OpenSettingsPanel()
    {
        int savedChunkSize = SettingsManager.LoadInt("ChunkSize", MazeGameManager.Instance.ChunkSize);
        int savedRenderDist = SettingsManager.LoadInt("RenderDistance", MazeGameManager.Instance.RenderDistance);

        if (_chunkSizeInput != null)
            _chunkSizeInput.text = savedChunkSize.ToString();

        if (_renderDistInput != null)
            _renderDistInput.text = savedRenderDist.ToString();

        _mainMenuContainer.SetActive(false);
        _settingsDialog.SetActive(true);
    }

    public void SaveSettings()
    {
        int cSize = 5;
        int rDist = 1;

        if (_chunkSizeInput != null && !int.TryParse(_chunkSizeInput.text, out cSize))
        {
            Debug.LogWarning($"Invalid chunk size input: '{_chunkSizeInput.text}', defaulting to 5");
            cSize = 5;
        }

        if (_renderDistInput != null && !int.TryParse(_renderDistInput.text, out rDist))
        {
            Debug.LogWarning($"Invalid render distance input: '{_renderDistInput.text}', defaulting to 1");
            rDist = 1;
        }

        cSize = Mathf.Max(1, cSize);
        rDist = Mathf.Max(1, rDist);

        MazeGameManager.Instance.ChunkSize = cSize;
        MazeGameManager.Instance.RenderDistance = rDist;

        // Save settings using SettingsManager
        SettingsManager.SaveInt("ChunkSize", cSize);
        SettingsManager.SaveInt("RenderDistance", rDist);

        _settingsDialog.SetActive(false);
        _mainMenuContainer.SetActive(true);
    }

    // --- CREDITS LOGIC (Open & Close) ---

    public void OpenCreditsPanel()
    {
        _mainMenuContainer.SetActive(false);
        _creditsDialog.SetActive(true);
    }

    public void CloseCreditsPanel()
    {
        _creditsDialog.SetActive(false);
        _mainMenuContainer.SetActive(true);
    }

    public void CancelSettings()
    {
        Debug.Log("Settings Cancelled");
        _settingsDialog.SetActive(false);
        _mainMenuContainer.SetActive(true);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}