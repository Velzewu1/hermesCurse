// ContagionSpray.cs (updated)
using UnityEngine;
using System.Collections;

/// <summary>
/// Менеджер способности Contagion Spray:
/// • Подписывается на событие OnContagion из AbilityUIController,
/// • Создаёт ContagionProjectile из shootPoint,
/// • Делегирует попадания AmbulanceHealth.
/// </summary>
public class ContagionSpray : MonoBehaviour
{
    public static ContagionSpray Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AbilityUIController uiController;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
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
        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogError("ContagionSpray: prefab or shootPoint not assigned", this);
            return;
        }
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