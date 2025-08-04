using UnityEngine;

/// <summary>
/// Визуальный снаряд для способности Contagion Spray.
/// Движется вперёд со скоростью speed, автоматически уничтожается через lifetime
/// и при столкновении с «скорая» сообщает ContagionSpray о попадании.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ContagionProjectile : MonoBehaviour
{
    [Tooltip("Скорость полёта снаряда (м/с)")]
    public float speed = 20f;
    [Tooltip("Время жизни снаряда (сек)")]
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // простое движение вперёд по локальной Z
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ambulance"))
        {
            // регистрируем попадание
            ContagionSpray.Instance?.RegisterHit(other.gameObject);
            // уничтожаем снаряд
            Destroy(gameObject);
        }
    }
}