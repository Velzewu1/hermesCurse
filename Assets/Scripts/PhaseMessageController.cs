using System.Collections;
using UnityEngine;
using TMPro;

public class PhaseMessageController : MonoBehaviour
{
    public static PhaseMessageController Instance { get; private set; }

    [Header("Message UI")]
    [Tooltip("Контейнер с текстом фазы")]
    [SerializeField] private GameObject messageContainer;
    [Tooltip("Текстовое поле (TMP)")]
    [SerializeField] private TextMeshProUGUI messageText;
    [Tooltip("Длительность показа (сек)")]
    [SerializeField] private float showDuration = 5f;

    [Header("Vignette (Q_Vignette_Single)")]
    [SerializeField] private Q_Vignette_Single vignette;
    [Tooltip("Время плавного нарастания/спада виньетки")]
    [SerializeField] private float rampTime = 1f;
    [Tooltip("Мин. масштаб виньетки")]
    [SerializeField] private float minScale = 0.1f;
    [Tooltip("Пик. масштаб виньетки")]
    [SerializeField] private float peakScale = 3f;
    [Tooltip("Частота пульсации (Гц)")]
    [SerializeField] private float pulseFreq = 2f;
    [Tooltip("Амплитуда пульсации (доля от peak)")]
    [SerializeField, Range(0f,1f)] private float pulseAmp = 0.1f;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        messageContainer?.SetActive(false);
        if (vignette) vignette.gameObject.SetActive(false);
    }

    /// <summary>
    /// Показать сообщение txt и анимированную виньетку в течение showDuration сек.
    /// </summary>
    public void ShowMessage(string txt)
    {
        if (messageContainer == null || messageText == null || vignette == null) return;

        // 1) Текст
        messageText.text = txt.ToUpperInvariant();
        messageContainer.SetActive(true);

        // 2) Подготовка виньетки
        vignette.mainScale = minScale;
        vignette.SetVignetteMainScale(minScale);
        vignette.SetVignetteSkyScale(minScale);
        vignette.gameObject.SetActive(true);

        // 3) Запуск корутины
        StopAllCoroutines();
        StartCoroutine(MessageRoutine());
    }

    private IEnumerator MessageRoutine()
    {
        float hold = Mathf.Max(0f, showDuration - 2f * rampTime);
        float t = 0f;

        // ramp-up
        while (t < rampTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(minScale, peakScale, t / rampTime);
            ApplyScale(s);
            yield return null;
        }

        // pulse
        float e = 0f;
        while (e < hold)
        {
            e += Time.deltaTime;
            float s = peakScale +
                      Mathf.Sin(e * Mathf.PI * 2f * pulseFreq) *
                      (peakScale * pulseAmp);
            ApplyScale(s);
            yield return null;
        }

        // ramp-down
        t = 0f;
        while (t < rampTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(peakScale, minScale, t / rampTime);
            ApplyScale(s);
            yield return null;
        }

        // конец
        messageContainer.SetActive(false);
        vignette.gameObject.SetActive(false);
    }

    private void ApplyScale(float s)
    {
        vignette.mainScale = s;
        vignette.SetVignetteMainScale(s);
        vignette.SetVignetteSkyScale(s);
    }
}
