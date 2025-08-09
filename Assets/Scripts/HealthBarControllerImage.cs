// HealthBarControllerImage.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class HealthBarControllerImage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthBarImage;          
    [SerializeField] private AmbulanceHealth ambulanceHealth;

    private GameObject container;
    private int maxHP;

    private void Awake()
    {
        // Находим контейнер и сразу скрываем
        container = healthBarImage.transform.parent.gameObject;
        container.SetActive(false);

        // Сохраняем максимальный HP
        maxHP = ambulanceHealth.CurrentHP;
    }

    private void OnEnable()
    {
        // Подписываемся на смену фаз
        var gm = GamePhaseManager.Instance;
        if (gm != null)
            gm.OnPhaseChanged.AddListener(OnPhaseChanged);
    }

    private void OnDisable()
    {
        // Отписываемся от смены фаз
        var gm = GamePhaseManager.Instance;
        if (gm != null)
            gm.OnPhaseChanged.RemoveListener(OnPhaseChanged);
        
        // Отписываемся от изменения HP (на случай, если уже подписались)
        ambulanceHealth.OnHealthChanged.RemoveListener(UpdateBar);
    }

    private void Start()
    {
        // Сразу синхронизируем состояние UI с текущей фазой
        var gm = GamePhaseManager.Instance;
        if (gm == null)
        {
            Debug.LogError("GamePhaseManager not found in scene!");
            enabled = false;
            return;
        }

        OnPhaseChanged(gm.CurrentPhase);
    }

    /// <summary>
    /// Вызывается при смене фазы игры.
    /// В финальной фазе показывает HP-бар и подписывается на изменение HP,
    /// в остальных прячет и отписывается.
    /// </summary>
    private void OnPhaseChanged(GamePhaseManager.Phase phase)
    {
        bool isEnd = phase == GamePhaseManager.Phase.End;

        if (isEnd)
        {
            // показываем контейнер и заполняем его
            container.SetActive(true);
            healthBarImage.fillAmount = 1f;
            ambulanceHealth.OnHealthChanged.AddListener(UpdateBar);
        }
        else
        {
            // прячем и отписываемся
            ambulanceHealth.OnHealthChanged.RemoveListener(UpdateBar);
            container.SetActive(false);
        }
    }

    /// <summary>
    /// Обновляет fillAmount при изменении HP скорой.
    /// </summary>
    private void UpdateBar(int current, int _)
    {
        healthBarImage.fillAmount = (float)current / maxHP;
    }
}