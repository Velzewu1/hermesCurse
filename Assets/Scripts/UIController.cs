using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public string exitSceneName = "MainMenu";

    private bool isPaused = false;

    void Update()
    {
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
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

    public void ExitToScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(exitSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
