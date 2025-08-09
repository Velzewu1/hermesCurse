using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneName; // Название сцены, на которую нужно переключиться
    [SerializeField] private float delay;

    private void Start()
    {
        StartCoroutine(SwitchSceneAfterDelay(delay));
    }

    private IEnumerator SwitchSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
