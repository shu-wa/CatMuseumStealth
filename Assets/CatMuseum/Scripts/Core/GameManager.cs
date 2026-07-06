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

    [Header("game state")]
    [SerializeField] private GameState currentState = GameState.Playing;

    [Header("game over")]
    [SerializeField] private bool pauseOnGameEnd = true;
    [SerializeField] private string gameOverMessage = "Game Over";

    [Header("clear")]
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
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (currentState == GameState.GameOver || currentState == GameState.Clear)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartCurrentScene();
            }
        }
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

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}