using System.Collections;
using UnityEngine;

/// <summary>
/// Активируется по событию OnCellPhase из AbilityUIController.
/// Делает игрока неуязвимым и анимирует виньетку (Q_Vignette_Single) плавным вхождением, пульсацией и спадом.
/// </summary>
public class CellPhase : MonoBehaviour
{
    [Header("Cell Phase Settings")]
    [Tooltip("Длительность фазы (сек)")]
    [SerializeField] private float duration = 10f;
    [Tooltip("Время плавного появления и исчезновения виньетки (сек)")]
    [SerializeField] private float rampTime = 1f;
    [Tooltip("Частота пульсации во время фазы (Гц)")]
    [SerializeField] private float pulsationFrequency = 2f;
    [Tooltip("Амплитуда пульсации, % от максимального масштаба")]
    [SerializeField, Range(0f, 1f)] private float pulsationAmplitude = 0.1f;

    private AbilityUIController uiController;
    private Q_Vignette_Single vignette;
    private bool isRunning;
    private float originalScale;

    private void Awake()
    {
        // Найти контроллер способностей
        uiController = GameObject.FindAnyObjectByType<AbilityUIController>();
        if (uiController == null)
            Debug.LogError("CellPhase: AbilityUIController not found", this);

        // Найти виньетку в сцене
        vignette = GameObject.FindAnyObjectByType<Q_Vignette_Single>();
        if (vignette == null)
            Debug.LogError("CellPhase: Q_Vignette_Single not found", this);
    }

    private void OnEnable()
    {
        if (uiController != null)
            uiController.OnCellPhase.AddListener(OnCellPhaseTriggered);
    }

    private void OnDisable()
    {
        if (uiController != null)
            uiController.OnCellPhase.RemoveListener(OnCellPhaseTriggered);
    }

    private void OnCellPhaseTriggered()
    {
        if (isRunning) return;
        StartCoroutine(CellPhaseRoutine());
    }

    private IEnumerator CellPhaseRoutine()
    {
        isRunning = true;
        PlayerDeath.IsInvulnerable = true;

        // Сохраняем исходный масштаб
        originalScale = vignette != null ? vignette.mainScale : 0f;

        // 1) Плавный рост масштаба 0→originalScale
        float t = 0f;
        while (t < rampTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(0f, originalScale, t / rampTime);
            if (vignette != null)
            {
                vignette.mainScale = s;
                vignette.SetVignetteMainScale(s);
                vignette.SetVignetteSkyScale(s);
            }
            yield return null;
        }

        // 2) Пульсация масштаба вокруг originalScale
        float holdTime = Mathf.Max(0f, duration - 2f * rampTime);
        float elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            float s = originalScale + Mathf.Sin(elapsed * Mathf.PI * 2f * pulsationFrequency) * (originalScale * pulsationAmplitude);
            if (vignette != null)
            {
                vignette.mainScale = s;
                vignette.SetVignetteMainScale(s);
                vignette.SetVignetteSkyScale(s);
            }
            yield return null;
        }

        // 3) Плавное исчезновение масштаба originalScale→0
        t = 0f;
        while (t < rampTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(originalScale, 0f, t / rampTime);
            if (vignette != null)
            {
                vignette.mainScale = s;
                vignette.SetVignetteMainScale(s);
                vignette.SetVignetteSkyScale(s);
            }
            yield return null;
        }

        // Завершение фазы
        PlayerDeath.IsInvulnerable = false;
        isRunning = false;
    }
}
