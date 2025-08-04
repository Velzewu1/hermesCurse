// HealthBarController.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Контролирует UI-бар HP скорой, появляется в фазе End.
/// </summary>
public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private AmbulanceHealth ambulanceHealth;

    private void Awake()
    {
        GamePhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
        // изначально скрыть
        healthBar.gameObject.SetActive(false);
    }

    private void OnPhaseChanged(GamePhaseManager.Phase phase)
    {
        if (phase == GamePhaseManager.Phase.End)
        {
            // показать и инициализировать
            healthBar.gameObject.SetActive(true);
            healthBar.maxValue = ambulanceHealth.CurrentHP;
            healthBar.value = ambulanceHealth.CurrentHP;
            ambulanceHealth.OnHealthChanged.AddListener(UpdateBar);
        }
        else
        {
            healthBar.gameObject.SetActive(false);
            ambulanceHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void UpdateBar(int current, int max)
    {
        healthBar.value = current;
    }
}