using UnityEngine;
using System.Collections;

/// <summary>
/// Менеджер способности Contagion Spray:
/// • Подписывается на событие OnContagion из AbilityUIController,
/// • Ждёт задержку перед выстрелом,
/// • Проигрывает звук и создаёт ContagionProjectile из shootPoint,
/// • Делегирует попадания AmbulanceHealth.
/// </summary>
public class ContagionSpray : MonoBehaviour
{
    public static ContagionSpray Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AbilityUIController uiController;
    [SerializeField] private GameObject           projectilePrefab;
    [SerializeField] private Transform            shootPoint;

    [Header("Fire Rate")]
    [Tooltip("Задержка между выстрелами (сек)")]
    [SerializeField] private float shootDelay = 0.5f;

    [Header("Audio")]
    [Tooltip("Звук выстрела Contagion Spray")]
    [SerializeField] private AudioClip contagionClip;

    private AudioSource audioSrc;
    private bool canShoot = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        // Получаем AudioSource на игроке
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            audioSrc = player.GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (uiController == null)
            uiController = GameObject.FindAnyObjectByType<AbilityUIController>();
        if (uiController != null)
            uiController.OnContagion.AddListener(TriggerSpray);
    }

    private void TriggerSpray()
    {
        if (!canShoot) return;
        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogError("ContagionSpray: prefab or shootPoint not assigned", this);
            return;
        }

        // Проиграть звук
        if (contagionClip != null && audioSrc != null)
            audioSrc.PlayOneShot(contagionClip);

        // Создать снаряд
        Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
    }


    /// <summary>
    /// Вызывается при попадании ContagionProjectile.
    /// Делегирует урон AmbulanceHealth.
    /// </summary>
    public void RegisterHit(GameObject ambulanceObj)
    {
        var health = ambulanceObj.GetComponent<AmbulanceHealth>();
        if (health != null)
            health.TakeDamage();
    }
}
