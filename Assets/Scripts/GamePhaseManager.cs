using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Cinemachine;

/// <summary>
/// 3-фазный таймер (по 60 с каждая).  
/// Фаза1 → Фаза2 → Фаза3 → End.
/// • На смене фазы повышает obstaclesPerSegment (6 → 7 → 8).  
/// • Выдаёт эвент <c>OnPhaseChanged</c> и вызывает
///   <c>UIManager.ShowPhaseMessage("Герпес мутирует")</c>.
/// </summary>
public class GamePhaseManager : MonoBehaviour
{
    public enum Phase { Phase1 = 0, Phase2 = 1, Phase3 = 2, End = 3 }

    [Header("References")]
    [SerializeField] private WorldManager worldManager;   // присвойте в инспекторе

    [Header("Timing (s)")]
    [SerializeField] private float phaseDuration = 60f;   // длительность каждой фазы

    public static GamePhaseManager Instance { get; private set; }
    public Phase CurrentPhase { get; private set; } = Phase.Phase1;
    public UnityEvent<Phase> OnPhaseChanged;              // подписка UI / способностей

    float timer;

    /* ───── Unity ───── */
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        timer = phaseDuration;
        ApplyPhaseSettings();                      // применяем 1-ю фазу
        OnPhaseChanged?.Invoke(CurrentPhase);      // даём знать подписчикам
        UIController.Instance?.ShowPhaseMessage("Герпес мутирует");
    }


    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f && CurrentPhase < Phase.End)
        {
            AdvancePhase();
        }
    }

    /* ───── helpers ───── */
    void AdvancePhase()
    {
        timer = phaseDuration;
        CurrentPhase++;
        ApplyPhaseSettings();
        OnPhaseChanged?.Invoke(CurrentPhase);

        // UI-подсказка (реализуется позже)
        UIController.Instance?.ShowPhaseMessage("Герпес мутирует");
    }

    void ApplyPhaseSettings()
    {
        if (!worldManager) return;

        switch (CurrentPhase)
        {
            case Phase.Phase1: worldManager.SetObstaclesPerSegment(3); break;
            case Phase.Phase2: worldManager.SetObstaclesPerSegment(5); break;
            case Phase.Phase3: worldManager.SetObstaclesPerSegment(6); break;
            default: break;
        }
    }
}
