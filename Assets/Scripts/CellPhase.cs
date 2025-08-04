using System.Collections;
using UnityEngine;

/// <summary>
/// Активируется по событию OnCellPhase из AbilityUIController.
/// Делает игрока неуязвимым и анимирует виньетку (Q_Vignette_Single) плавным вхождением, пульсацией и спадом.
/// В конце, перед тем как масштаб дойдёт до minScale, отключает сам GameObject виньетки.
/// </summary>
public class CellPhase : MonoBehaviour
{
    [Header("Cell Phase Settings")]
    [SerializeField] private Q_Vignette_Single vignette;
    [Tooltip("Длительность фазы (сек)")]
    [SerializeField] private float duration = 10f;
    [Tooltip("Время плавного появления и исчезновения виньетки (сек)")]
    [SerializeField] private float rampTime = 1f;
    [Tooltip("Частота пульсации (Гц)")]
    [SerializeField] private float pulsationFrequency = 2f;
    [Tooltip("Минимальный масштаб виньетки (неизменяемый после спада)")]
    [SerializeField] private float minScale = 0.1f;
    [Tooltip("Амплитуда пульсации, доля от пика")]
    [SerializeField, Range(0f, 1f)] private float pulsationAmplitude = 0.1f;

    private AbilityUIController uiController;
    private bool isRunning;

    private void Awake()
    {
        uiController = GameObject.FindAnyObjectByType<AbilityUIController>();
        if (uiController == null)
            Debug.LogError("CellPhase: AbilityUIController not found", this);

        if (vignette == null)
            Debug.LogError("CellPhase: Q_Vignette_Single not found", this);
        else
            vignette.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (uiController != null)
            uiController.OnCellPhase.AddListener(TriggerCellPhase);
    }

    private void OnDisable()
    {
        if (uiController != null)
            uiController.OnCellPhase.RemoveListener(TriggerCellPhase);
    }

    private void TriggerCellPhase()
    {
        if (isRunning) return;
        StartCoroutine(CellPhaseRoutine());
    }

    private IEnumerator CellPhaseRoutine()
    {
        isRunning = true;
        PlayerDeath.IsInvulnerable = true;

        const float peakScale = 3f;

        // 1) Плавный рост minScale → peakScale
        float t = 0f;
        while (t < rampTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(minScale, peakScale, t / rampTime);
            ApplyScale(s);
            yield return null;
        }

        // 2) Пульсация вокруг peakScale
        float holdTime = Mathf.Max(0f, duration - 2f * rampTime);
        float elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            float s = peakScale + Mathf.Sin(elapsed * Mathf.PI * 2f * pulsationFrequency) * (peakScale * pulsationAmplitude);
            ApplyScale(s);
            yield return null;
        }

        // 3) Плавный спад peakScale → minScale
        t = 0f;
        while (t < rampTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(peakScale, minScale, t / rampTime);
            ApplyScale(s);
            yield return null;
        }

        // Отключаем объект виньетки до того, как он станет полностью невидим
        if (vignette != null)
            vignette.gameObject.SetActive(false);

        PlayerDeath.IsInvulnerable = false;
        isRunning = false;
    }

    private void ApplyScale(float s)
    {
        if (vignette == null) return;
        if (!vignette.gameObject.activeSelf)
            vignette.gameObject.SetActive(true);
        vignette.mainScale = s;
        vignette.SetVignetteMainScale(s);
        vignette.SetVignetteSkyScale(s);
    }
}
