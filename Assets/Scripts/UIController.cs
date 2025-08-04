using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private string exitSceneName = "MainMenu";

    bool isPaused;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        pauseMenuCanvas?.SetActive(false);
    }

    #region Pause
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else          PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuCanvas?.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuCanvas?.SetActive(false);
        isPaused = false;
    }
    #endregion

    #region Navigation
    public void ExitToScene()    => SceneManager.LoadScene(exitSceneName);
    public void AfterVideo()     => SceneManager.LoadScene("Video");
    public void QuitGame()       => Application.Quit();
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
