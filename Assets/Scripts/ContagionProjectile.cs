using UnityEngine;
using System.Collections;

/// <summary>
/// Визуальный снаряд для способности Contagion Spray.
/// • Двигается вперёд со скоростью speed,
/// • при запуске спавнит и запускает launchEffect,
/// • при попадании срабатывает hitEffect,
/// • автоматически уничтожается через lifetime.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ContagionProjectile : MonoBehaviour
{
    [Tooltip("Скорость полёта снаряда (м/с)")]
    public float speed = 20f;
    [Tooltip("Время жизни снаряда (сек)")]
    public float lifetime = 5f;

    [Header("Effects")]
    [Tooltip("Префаб эффекта при запуске снаряда")]
    [SerializeField] private ParticleSystem launchEffectPrefab;
    [Tooltip("Префаб эффекта при попадании")]
    [SerializeField] private ParticleSystem hitEffectPrefab;

    private ParticleSystem launchEffectInstance;

    private void Start()
    {
        // запускаем эффект при старте
        if (launchEffectPrefab)
        {
            launchEffectInstance = Instantiate(
                launchEffectPrefab,
                transform.position,
                transform.rotation,
                transform);
            launchEffectInstance.Play();
        }
        // автоматическое уничтожение
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // движемся вперёд
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ambulance"))
        {
            // эффект попадания
            if (hitEffectPrefab)
            {
                var hitEffect = Instantiate(
                    hitEffectPrefab,
                    transform.position,
                    Quaternion.identity);
                hitEffect.Play();
            }
            // сообщаем о попадании
            ContagionSpray.Instance?.RegisterHit(other.gameObject);
            // остановить запущенный эффект и уничтожить
            if (launchEffectInstance)
            {
                launchEffectInstance.Stop();
                launchEffectInstance.transform.SetParent(null);
                Destroy(launchEffectInstance.gameObject, 2f);
            }
            Destroy(gameObject);
        }
    }
}
