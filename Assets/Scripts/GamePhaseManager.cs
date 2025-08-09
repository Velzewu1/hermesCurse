// GamePhaseManager.cs
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 3‑фазный таймер (по phaseDuration сек каждая).
/// Phases: Phase1 → Phase2 → Phase3 → End.
/// При смене фазы:
/// • увеличивает obstaclesPerSegment в WorldManager;
/// • вызывает OnPhaseChanged;
/// • выводит сообщение через PhaseMessageController;
/// • проигрывает звуковой клип от позиции MainCamera.
/// </summary>
public class GamePhaseManager : MonoBehaviour
{
    public enum Phase { Phase1 = 0, Phase2 = 1, Phase3 = 2, End = 3 }

    [Header("References")]
    [SerializeField] private WorldManager worldManager;

    [Header("Timing (s)")]
    [SerializeField] private float phaseDuration = 60f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip phase1Clip;
    [SerializeField] private AudioClip phase2Clip;
    [SerializeField] private AudioClip phase3Clip;
    [SerializeField] private AudioClip endPhaseClip;

    public static GamePhaseManager Instance { get; private set; }
    public Phase CurrentPhase { get; private set; } = Phase.Phase1;
    public UnityEvent<Phase> OnPhaseChanged = new UnityEvent<Phase>();

    private float timer;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        OnPhaseChanged = OnPhaseChanged ?? new UnityEvent<Phase>();
    }

    private void Start()
    {
        timer = phaseDuration;
        ApplyPhaseSettings();
        OnPhaseChanged.Invoke(CurrentPhase);
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
    }

    private void ApplyPhaseSettings()
    {
        if (worldManager == null) return;

        string msg = string.Empty;
        AudioClip clip = null;

        switch (CurrentPhase)
        {
            case Phase.Phase1:
                msg = "SYMBIOSIS ESTABLISHED";
                worldManager.SetObstaclesPerSegment(3);
                clip = phase1Clip;
                break;
            case Phase.Phase2:
                msg = "HERMES IS MUTATING CELL PHASE";
                worldManager.SetObstaclesPerSegment(5);
                clip = phase2Clip;
                break;
            case Phase.Phase3:
                msg = "HERMES IS MUTATING VIRAL OVERCLOCK";
                worldManager.SetObstaclesPerSegment(5);
                clip = phase3Clip;
                break;
            case Phase.End:
                msg = "HERMES IS MUTATING KILL EM ALL";
                worldManager.SetObstaclesPerSegment(7);
                clip = endPhaseClip;
                break;
        }

        PhaseMessageController.Instance?.ShowMessage(msg);

        if (clip != null)
        {
            var cam = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
            if (cam != null)
                cam.PlayOneShot(clip);
        }
    }
}
