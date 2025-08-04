using System.Collections;
using UnityEngine;

/// <summary>
/// Активируется по событию OnViralOverclock из AbilityUIController.
/// Увеличивает высоту прыжка и ослабляет гравитацию игрока в течение фазы,
/// а также анимирует заданную виньетку (Q_Vignette_Single) плавным вхождением,
/// пульсацией и спадом.
/// </summary>
public class ViralOverclock : MonoBehaviour
{
    [Header("Overclock Settings")]
    [SerializeField] private Q_Vignette_Single vignette;
    [Tooltip("Длительность фазы (сек)")]
    [SerializeField] private float duration = 10f;
    [Tooltip("Время плавного появления/исчезновения виньетки (сек)")]
    [SerializeField] private float rampTime = 1f;
    [Tooltip("Частота пульсации (Гц)")]
    [SerializeField] private float pulsationFrequency = 2f;
    [Tooltip("Минимальный масштаб виньетки (чтобы не было нуля)")]
    [SerializeField] private float minScale = 0.1f;
    [Tooltip("Амплитуда пульсации, доля от пика")]
    [SerializeField, Range(0f,1f)] private float pulsationAmplitude = 0.1f;

    [Header("Player Modifiers")]
    [Tooltip("Множитель высоты прыжка")]
    [SerializeField] private float jumpMultiplier = 3f;
    [Tooltip("Множитель гравитации (1 = нет изменения)")]
    [SerializeField] private float gravityMultiplier = 0.5f;

    private AbilityUIController uiController;
    private PlayerControls playerControls;
    private bool isRunning;

    private void Awake()
    {
        uiController = GameObject.FindAnyObjectByType<AbilityUIController>();
        if (uiController == null)
            Debug.LogError("ViralOverclock: AbilityUIController not found", this);

        playerControls = GameObject.FindAnyObjectByType<PlayerControls>();
        if (playerControls == null)
            Debug.LogError("ViralOverclock: PlayerControls not found", this);

        if (vignette == null)
            Debug.LogError("ViralOverclock: Vignette reference not assigned", this);
        else
            vignette.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (uiController != null)
            uiController.OnViralOverclock.AddListener(TriggerOverclock);
    }

    private void OnDisable()
    {
        if (uiController != null)
            uiController.OnViralOverclock.RemoveListener(TriggerOverclock);
    }

    private void TriggerOverclock()
    {
        if (isRunning) return;
        StartCoroutine(OverclockRoutine());
    }

    private IEnumerator OverclockRoutine()
    {
        isRunning = true;

        // сохраняем оригинальные значения
        float origJump  = playerControls.jumpHeight;
        float origGravity = playerControls.gravity;

        // устанавливаем усиленные параметры
        playerControls.jumpHeight = origJump * jumpMultiplier;
        playerControls.gravity    = origGravity * gravityMultiplier;

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
            float s = peakScale + Mathf.Sin(elapsed * Mathf.PI * 2f * pulsationFrequency)
                      * (peakScale * pulsationAmplitude);
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

        // скрываем ветнетку и восстанавливаем параметры
        if (vignette != null)
            vignette.gameObject.SetActive(false);

        playerControls.jumpHeight = origJump;
        playerControls.gravity    = origGravity;
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
