using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Вешается на префаб <b>Car</b> (коллайдер – Trigger).<br/>
/// При контакте со скорой или игроком:
/// 1) создаёт префаб взрыва и делает его дочерним;<br/>
/// 2) скрывает визуал машины и отключает её коллайдеры;<br/>
/// 3) через <see cref="destroyDelay"/> секунд удаляет объект;<br/>
/// 4) воспроизводит звуковой эффект при взрыве.
/// </summary>
public class Exploder : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("Префаб эффекта взрыва (опционально)")]
    [SerializeField] private GameObject explosionPrefab;
    [Tooltip("Через сколько секунд удалить машину после взрыва")]
    [SerializeField] private float destroyDelay = 10f;

    [Header("Audio Settings")]
    [Tooltip("Звуковой эффект при взрыве (опционально)")]
    [SerializeField] private AudioClip explosionSFX;

    private AudioSource audioSource;
    private bool hasExploded;

    private void Awake()
    {
        // Получаем или создаём AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 0) звук взрыва
        if (explosionSFX != null)
            audioSource.PlayOneShot(explosionSFX);

        // 1) скрыть визуал и выключить коллайдеры ДО инстанса взрыва
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (var c in GetComponents<Collider>())
            c.enabled = false;

        // 2) эффект (теперь не будет выключен)
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity, transform);

        // 3) удалить после паузы
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ambulance") || other.CompareTag("Player"))
        {
            other.GetComponent<PlayerDeath>()?.Die();
            Explode();
            FindAnyObjectByType<ShockShooter>().DropCollectible();
        }
    }
}
