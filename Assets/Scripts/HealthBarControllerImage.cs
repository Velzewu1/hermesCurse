using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Контролирует UI-бар HP скорой, используя Image.fillAmount.
/// Родительский объект контейнера остаётся скрытым до последней фазы (End).
/// В фазе End: показывает контейнер, заполняет бара до maxHP и уменьшает по событиям.
/// </summary>
public class HealthBarControllerImage : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Image с типом Fill для HP-бара")]  
    [SerializeField] private Image healthBarImage;

    [Tooltip("Компонент AmbulanceHealth на скорой помощи")]  
    [SerializeField] private AmbulanceHealth ambulanceHealth;

    private GameObject container;
    private int maxHP;

    private void Awake()
    {
        // контейнер — родитель image
        if (healthBarImage != null)
            container = healthBarImage.transform.parent.gameObject;

        // скрыть контейнер до последней фазы
        if (container != null)
            container.SetActive(false);

        // запомним maxHP
        if (ambulanceHealth != null)
            maxHP = ambulanceHealth.CurrentHP;

        // подписка на смену фаз
        GamePhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
    }

    private void OnPhaseChanged(GamePhaseManager.Phase phase)
    {
        bool isEnd = phase == GamePhaseManager.Phase.End;      

        if (isEnd)
        {
            container.SetActive(true);
            // инициализируем бар полным
            if (healthBarImage != null)
                healthBarImage.fillAmount = 1f;

            // подписываемся на изменение HP
            ambulanceHealth.OnHealthChanged.AddListener(UpdateBar);
        }
        else
        {
            // отписываемся
            ambulanceHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void UpdateBar(int current, int max)
    {
        if (healthBarImage != null && maxHP > 0)
        {
            healthBarImage.fillAmount = (float)current / maxHP;
        }
    }
}
