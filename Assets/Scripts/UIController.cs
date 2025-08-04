using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }
    public GameObject mutate;
    public GameObject pauseMenuCanvas;
    public string exitSceneName = "MainMenu";

    bool isPaused;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /* ---------- Pause ---------- */
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuCanvas.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuCanvas.SetActive(false);
        isPaused = false;
    }

    /* ---------- Navigation ---------- */
    public void ExitToScene() => SceneManager.LoadScene(exitSceneName);

    public void QuitGame() => Application.Quit();

    public void RestartScene()
    {
        Time.timeScale = 1f; // обязательно сбрасываем перед рестартом
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /* ---------- Phase message (stub) ---------- */
    public void ShowPhaseMessage(string txt)
    {
        mutate.SetActive(true);
    }
}
