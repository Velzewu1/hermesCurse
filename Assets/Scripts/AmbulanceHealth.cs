using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Хранит HP скорой помощи и выдаёт событие при изменении.
/// При достижении 0 HP загружает сцену "TheEnd".
/// </summary>
public class AmbulanceHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHP = 5;
    public int CurrentHP { get; private set; }
    public UnityEvent<int, int> OnHealthChanged; // args: current, max

    private void Awake()
    {
        CurrentHP = maxHP;
        OnHealthChanged = OnHealthChanged ?? new UnityEvent<int, int>();
    }

    /// <summary>Применить 1 единицу урона.</summary>
    public void TakeDamage()
    {
        if (CurrentHP <= 0) return;

        CurrentHP--;
        OnHealthChanged.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0)
        {
            // Взрыв скорой
            GetComponent<Exploder>()?.Explode();

            // Переход на финальную сцену
            SceneManager.LoadScene("TheEnd");
        }
    }
}
