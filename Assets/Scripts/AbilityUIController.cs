using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Управление UI-кнопками способностей и их активацией.
/// Проигрывает звук при активации первых двух способностей.
/// Контейнер для Contagion теперь имеет свой откат на кнопку, ресурс тратится лишь раз за откат.
/// </summary>
public class AbilityUIController : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button cellPhaseBtn;
    [SerializeField] private Button viralOverclockBtn;
    [SerializeField] private Button contagionBtn;

    [Header("Audio Clips")]
    [Tooltip("Звук при активации Cell Phase")]
    [SerializeField] private AudioClip cellPhaseClip;
    [Tooltip("Звук при активации Viral Overclock")]
    [SerializeField] private AudioClip viralOverclockClip;
    [Tooltip("Звук при активации Contagion Spray")]
    [SerializeField] private AudioClip contagionClip;

    [Header("Contagion Cooldown")]
    [Tooltip("Откат кнопки Contagion (сек)")]
    [SerializeField] private float contagionCooldown = 1f;

    [Header("Ability Events")]
    public UnityEvent OnCellPhase;
    public UnityEvent OnViralOverclock;
    public UnityEvent OnContagion;

    private AudioSource audioSrc;
    private bool canContagion = true;

    private void Awake()
    {
        // Найдём AudioSource на камере игрока
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            audioSrc = player.GetComponent<AudioSource>();
    }

    private void Start()
    {
        cellPhaseBtn.onClick.AddListener(() => ActivateAbility(1));
        viralOverclockBtn.onClick.AddListener(() => ActivateAbility(2));
        contagionBtn.onClick.AddListener(() => ActivateAbility(3));

        GamePhaseManager.Instance.OnPhaseChanged.AddListener(UpdateButtonStates);
        CollectManager.Instance.OnValueChanged += _ => UpdateButtonStates(GamePhaseManager.Instance.CurrentPhase);

        UpdateButtonStates(GamePhaseManager.Instance.CurrentPhase);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame) ActivateAbility(1);
        if (kb.digit2Key.wasPressedThisFrame) ActivateAbility(2);
        if (kb.digit3Key.wasPressedThisFrame) ActivateAbility(3);
    }

    private void ActivateAbility(int idx)
    {
        var phase = GamePhaseManager.Instance.CurrentPhase;
        // разблокировка по фазе
        if ((idx == 1 && phase < GamePhaseManager.Phase.Phase2) ||
            (idx == 2 && phase < GamePhaseManager.Phase.Phase3) ||
            (idx == 3 && phase < GamePhaseManager.Phase.End))
            return;

        // для Contagion добавляем проверку отката
        if (idx == 3 && !canContagion) return;

        if (!AbilityBank.Instance.TrySpend(idx)) return;

        // звук
        if (audioSrc != null)
        {
            if (idx == 1 && cellPhaseClip != null)           audioSrc.PlayOneShot(cellPhaseClip);
            else if (idx == 2 && viralOverclockClip != null)  audioSrc.PlayOneShot(viralOverclockClip);
            else if (idx == 3 && contagionClip != null)       audioSrc.PlayOneShot(contagionClip);
        }

        switch (idx)
        {
            case 1: OnCellPhase?.Invoke();        break;
            case 2: OnViralOverclock?.Invoke();  break;
            case 3:
                OnContagion?.Invoke();
                canContagion = false;
                StartCoroutine(ResetContagionCooldown());
                break;
        }
    }

    private IEnumerator ResetContagionCooldown()
    {
        yield return new WaitForSeconds(contagionCooldown);
        canContagion = true;
    }

    private void UpdateButtonStates(GamePhaseManager.Phase newPhase)
    {
        var cm      = CollectManager.Instance;
        bool barFull= cm.IsFull;
        int  count  = cm.Count;

        bool show1  = newPhase >= GamePhaseManager.Phase.Phase2;
        bool show2  = newPhase >= GamePhaseManager.Phase.Phase3;
        bool show3  = newPhase == GamePhaseManager.Phase.End;

        cellPhaseBtn.gameObject.SetActive(show1);
        viralOverclockBtn.gameObject.SetActive(show2);
        contagionBtn.gameObject.SetActive(show3);

        cellPhaseBtn.interactable        = show1 && barFull;
        viralOverclockBtn.interactable  = show2 && barFull;
        contagionBtn.interactable       = show3 && count > 0 && canContagion;
    }
}
