using UnityEngine;
using TMPro;

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
    [SerializeField] private TMP_InputField _chunkSizeInput;
    [SerializeField] private TMP_InputField _renderDistInput;

    private void Start()
    {
        GameSession.End();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowOnly(_mainMenuContainer);
    }

    public void OpenNewGamePanel() => ShowOnly(_newGameDialog);
    public void CloseNewGamePanel() => ShowOnly(_mainMenuContainer);

    public void StartGame()
    {
        GameSession.Begin(MatchConfig.FromInput(_mazeXInput.text, _mazeYInput.text, _mazeZInput.text, _seedInput.text));
        SceneLoader.Load("GameScene");
    }

    public void OpenSettingsPanel()
    {
        if (_chunkSizeInput != null) _chunkSizeInput.text = LocalSettings.ChunkSize.ToString();
        if (_renderDistInput != null) _renderDistInput.text = LocalSettings.RenderDistance.ToString();
        ShowOnly(_settingsDialog);
    }

    public void SaveSettings()
    {
        if (_chunkSizeInput != null)
            LocalSettings.ChunkSize = int.TryParse(_chunkSizeInput.text, out int c) ? c : LocalSettings.DefaultChunkSize;
        if (_renderDistInput != null)
            LocalSettings.RenderDistance = int.TryParse(_renderDistInput.text, out int r) ? r : LocalSettings.DefaultRenderDistance;
        ShowOnly(_mainMenuContainer);
    }

    public void CancelSettings() => ShowOnly(_mainMenuContainer);

    public void OpenCreditsPanel() => ShowOnly(_creditsDialog);
    public void CloseCreditsPanel() => ShowOnly(_mainMenuContainer);

    public void QuitGame() => Application.Quit();

    private void ShowOnly(GameObject panel)
    {
        if (_mainMenuContainer) _mainMenuContainer.SetActive(panel == _mainMenuContainer);
        if (_newGameDialog) _newGameDialog.SetActive(panel == _newGameDialog);
        if (_settingsDialog) _settingsDialog.SetActive(panel == _settingsDialog);
        if (_creditsDialog) _creditsDialog.SetActive(panel == _creditsDialog);
    }
}
