// AmbulanceHealth.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class AmbulanceHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)] private int maxHP = 5;

    /// <summary>Публичное текущее здоровье.</summary>
    public int CurrentHP { get; private set; }

    /// <summary>Событие: (current, max).</summary>
    public UnityEvent<int,int> OnHealthChanged = new UnityEvent<int,int>();

    [Header("Death Settings")]
    [Tooltip("Префаб взрыва")]
    [SerializeField] private GameObject explosionPrefab;
    [Tooltip("Задержка перед переходом на TheEnd (сек)")]
    [SerializeField] private float deathDelay = 1f;

    private bool isDead;

    private void Awake()
    {
        // Инициализируем здоровье и сразу рассылаем текущее значение
        CurrentHP = maxHP;
        OnHealthChanged.Invoke(CurrentHP, maxHP);
    }

    /// <summary>Наносит 1 урона.</summary>
    public void TakeDamage()
    {
        if (isDead) return;

        CurrentHP = Mathf.Max(CurrentHP - 1, 0);
        OnHealthChanged.Invoke(CurrentHP, maxHP);

        if (CurrentHP == 0)
        {
            isDead = true;
            StartCoroutine(HandleDeath());
        }
    }

    private IEnumerator HandleDeath()
    {
        // 1) Взорвать
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // 2) Ждать
        yield return new WaitForSeconds(deathDelay);

        // 3) Финальная сцена
        SceneManager.LoadScene("TheEnd");
    }
}
