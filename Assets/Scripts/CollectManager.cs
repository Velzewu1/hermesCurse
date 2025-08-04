using System;
using UnityEngine;
using UnityEngine.UI;

public class CollectManager : MonoBehaviour
{
    public static CollectManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image bar;          
    [SerializeField, Min(1)] private int maxCount = 10;

    public int Count { get; private set; }

    /// <summary>Fires whenever Count changes.</summary>
    public event Action<int> OnValueChanged;

    public int MaxCount => maxCount;
    public bool IsFull   => Count >= maxCount;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
        OnValueChanged?.Invoke(Count);
    }

    public void AddCollectible(int amount = 1)
    {
        Count = Mathf.Clamp(Count + amount, 0, maxCount);
        UpdateUI();
        OnValueChanged?.Invoke(Count);
    }

    /// <summary>Attempts to spend exactly `amount`. Returns true if successful.</summary>
    public bool UseCollectible(int amount)
    {
        if (Count < amount) return false;
        Count -= amount;
        UpdateUI();
        OnValueChanged?.Invoke(Count);
        return true;
    }

    private void UpdateUI()
    {
        if (bar) bar.fillAmount = (float)Count / maxCount;
    }
}
