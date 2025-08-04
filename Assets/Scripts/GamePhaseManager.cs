// GamePhaseManager.cs
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 3-фазный таймер (по phaseDuration сек каждая).  
/// Phase1 → Phase2 → Phase3 → End.
/// • На смене фазы повышает obstaclesPerSegment в WorldManager (6→7→8).  
/// • Выдаёт эвент OnPhaseChanged и сразу при старте уведомляет об Phase1.  
/// • Вызывает UIController.ShowPhaseMessage("Герпес мутирует").
/// </summary>
public class GamePhaseManager : MonoBehaviour
{
    public enum Phase { Phase1 = 0, Phase2 = 1, Phase3 = 2, End = 3 }

    [Header("References")]
    [SerializeField] private WorldManager worldManager;

    [Header("Timing (s)")]
    [SerializeField] private float phaseDuration = 60f;

    public static GamePhaseManager Instance { get; private set; }
    public Phase CurrentPhase { get; private set; } = Phase.Phase1;
    public UnityEvent<Phase> OnPhaseChanged = new UnityEvent<Phase>();

    private float timer;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        OnPhaseChanged = OnPhaseChanged ?? new UnityEvent<Phase>();
    }

    private void Start()
    {
        timer = phaseDuration;
        ApplyPhaseSettings();
        // сразу уведомляем об Phase1
        OnPhaseChanged.Invoke(CurrentPhase);
        PhaseMessageController.Instance?.ShowMessage("HERMES IS MUTATING");
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f && CurrentPhase < Phase.End)
            AdvancePhase();
    }

    private void AdvancePhase()
    {
        timer = phaseDuration;
        CurrentPhase++;
        ApplyPhaseSettings();
        OnPhaseChanged.Invoke(CurrentPhase);
        PhaseMessageController.Instance?.ShowMessage("HERMES IS MUTATING");
    }

    private void ApplyPhaseSettings()
    {
        if (worldManager == null) return;
        switch (CurrentPhase)
        {
            case Phase.Phase1:
                worldManager.SetObstaclesPerSegment(1);
                break;
            case Phase.Phase2:
                worldManager.SetObstaclesPerSegment(2);
                break;
            case Phase.Phase3:
                worldManager.SetObstaclesPerSegment(3);
                break;
            default:
                worldManager.SetObstaclesPerSegment(7);
                break;
        }
    }
}
