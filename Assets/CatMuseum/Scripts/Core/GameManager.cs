using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Clear
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("game state")]
    [SerializeField] private GameState currentState = GameState.Playing;

    [Header("game end")]
    [SerializeField] private bool pauseOnGameEnd = true;
    [SerializeField] private string gameOverMessage = "Game Over";
    [SerializeField] private string clearMessage = "Clear!";

    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
    public bool IsGameOver => currentState == GameState.GameOver;
    public bool IsClear => currentState == GameState.Clear;

    public string GameOverMessage => gameOverMessage;
    public string ClearMessage => clearMessage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (currentState == GameState.GameOver || currentState == GameState.Clear)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReturnToMainMenu();
            }
        }
    }

    public void StartMission(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty");
            return;
        }

        currentState = GameState.Playing;
        Time.timeScale = 1f;

        LockCursor();
        SceneManager.LoadScene(sceneName);
    }

    public void StartSelectedMission()
    {
        if (PlayerProfile.Instance == null)
        {
            Debug.LogWarning("PlayerProfile is not found");
            return;
        }

        StartMission(PlayerProfile.Instance.SelectedMapSceneName);
    }

    public void GameOver(string message)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.GameOver;
        gameOverMessage = message;

        Debug.Log("GAME OVER: " + message);

        UnlockCursor();

        if (pauseOnGameEnd)
        {
            Time.timeScale = 0f;
        }
    }

    public void ClearGame(string message)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.Clear;
        clearMessage = message;

        Debug.Log("GAME CLEAR: " + message);

        UnlockCursor();

        if (pauseOnGameEnd)
        {
            Time.timeScale = 0f;
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        currentState = GameState.Playing;

        UnlockCursor();
        SceneManager.LoadScene(mainMenuSceneName);
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
}