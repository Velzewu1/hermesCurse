using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }   // <─ NEW

    public GameObject pauseMenuCanvas;
    public string exitSceneName = "MainMenu";

    bool isPaused;

    void Awake()                 // <─ NEW
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /* optional Update removed */

    /* ---------- Pause ---------- */
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else          PauseGame();
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
    public void QuitGame()    => Application.Quit();

    /* ---------- Phase message (stub) ---------- */
    public void ShowPhaseMessage(string txt)      // <─ NEW
    {
        Debug.Log($"[UI] {txt}");                 // позже заменим на реальный HUD
    }
}
