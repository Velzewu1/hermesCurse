// HealthBarControllerImage.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class HealthBarControllerImage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthBarImage;          // Image с типом Filled
    [SerializeField] private AmbulanceHealth ambulanceHealth;

    private GameObject container;
    private int maxHP;

    private void Awake()
    {
        container = healthBarImage.transform.parent.gameObject;
        container.SetActive(false);
        maxHP = ambulanceHealth.CurrentHP;
        GamePhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
    }

    private void OnPhaseChanged(GamePhaseManager.Phase phase)
    {
        if (phase == GamePhaseManager.Phase.End)
        {
            container.SetActive(true);
            healthBarImage.fillAmount = 1f;
            ambulanceHealth.OnHealthChanged.AddListener(UpdateBar);
        }
        else
        {
            ambulanceHealth.OnHealthChanged.RemoveListener(UpdateBar);
            container.SetActive(false);
        }
    }

    private void UpdateBar(int current, int _)
    {
        healthBarImage.fillAmount = (float)current / maxHP;
    }
}
