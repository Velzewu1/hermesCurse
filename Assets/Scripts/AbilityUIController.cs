using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AbilityUIController : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button cellPhaseBtn;
    [SerializeField] private Button viralOverclockBtn;
    [SerializeField] private Button contagionBtn;

    [Header("Ability Events")]
    public UnityEvent OnCellPhase;
    public UnityEvent OnViralOverclock;
    public UnityEvent OnContagion;

    private void Start()
    {
        // wire up clicks
        cellPhaseBtn.onClick.AddListener(() => ActivateAbility(1));
        viralOverclockBtn.onClick.AddListener(() => ActivateAbility(2));
        contagionBtn.onClick.AddListener(() => ActivateAbility(3));

        // phase‐change callback
        GamePhaseManager.Instance.OnPhaseChanged.AddListener(UpdateButtonStates);

        // resource‐change callback
        CollectManager.Instance.OnValueChanged += _ => UpdateButtonStates(GamePhaseManager.Instance.CurrentPhase);

        // initial UI state
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
        // unlock logic
        var phase = GamePhaseManager.Instance.CurrentPhase;
        if ((idx == 1 && phase < GamePhaseManager.Phase.Phase2) ||
            (idx == 2 && phase < GamePhaseManager.Phase.Phase3) ||
            (idx == 3 && phase < GamePhaseManager.Phase.End))
            return;

        if (!AbilityBank.Instance.TrySpend(idx)) return;

        switch (idx)
        {
            case 1: OnCellPhase?.Invoke();        break;
            case 2: OnViralOverclock?.Invoke();  break;
            case 3: OnContagion?.Invoke();       break;
        }
    }

    private void UpdateButtonStates(GamePhaseManager.Phase newPhase)
    {
        var cm = CollectManager.Instance;
        bool barFull = cm.IsFull;
        int  count   = cm.Count;

        // determine visibility
        bool showCell     = newPhase >= GamePhaseManager.Phase.Phase2;
        bool showViral    = newPhase >= GamePhaseManager.Phase.Phase3;
        bool showContagion= newPhase == GamePhaseManager.Phase.End;

        // show/hide
        cellPhaseBtn.gameObject.SetActive(showCell);
        viralOverclockBtn.gameObject.SetActive(showViral);
        contagionBtn.gameObject.SetActive(showContagion);

        // interactable only if you can cast
        cellPhaseBtn.interactable       = showCell     && barFull;
        viralOverclockBtn.interactable = showViral    && barFull;
        contagionBtn.interactable      = showContagion && count > 0;
    }
}
