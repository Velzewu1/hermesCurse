using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Синглтон, хранит текущее количество собранных предметов и отражает
/// значение в UI-шкале (Image.fillAmount).
/// </summary>
public class CollectManager : MonoBehaviour
{
    public static CollectManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Image-шкала (тип Filled, Fill Method = Horizontal)")]
    [SerializeField] private Image bar;          // заполняем в инспекторе
    [Tooltip("Сколько collectible нужно, чтобы шкала была полной")]
    [SerializeField] private int   maxCount = 10;

    int count;                                   // текущее значение

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddCollectible(int value = 1)
    {
        count = Mathf.Min(count + value, maxCount);
        UpdateUI();
    }

    /* ───── helpers ───── */
    void UpdateUI()
    {
        if (bar)
            bar.fillAmount = (float)count / maxCount;
    }

    /* сброс шкалы между фазами, если понадобится */
    public void ResetCounter()
    {
        count = 0;
        UpdateUI();
    }
}
