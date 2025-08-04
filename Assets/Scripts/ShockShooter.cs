using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

/// <summary>
/// • 4 с красный прицельный луч (растёт в ширину, «догоняет» игрока).<br/>
/// • 0.45 с толстая молния ×3 шире, Raycast ровно до точки удара → ваншот.<br/>
/// • Экранная отдача + краткая хроматика; камера больше не отключается.<br/>
/// Поместите скрипт на пустой ShootPoint с LineRenderer-ом.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class ShockShooter : MonoBehaviour
{
    /* ───── материалы ───── */
    [Header("Materials")]
    [SerializeField] private Material lightningMat;                // молния
    [SerializeField] private Material aimMat;                      // красный луч

    /* ───── тайминги ───── */
    [Header("Timing (s)")]
    [SerializeField] private float aimDuration = 4f;
    [SerializeField] private float followLag = 3f;
    [SerializeField] private float lightningTime = 0.45f;
    [SerializeField] private float cooldown = 4f;

    /* ───── поведение луча ───── */
    [Header("Laser look")]
    [SerializeField] private float baseWidth = 0.12f;              // макс. ширина прицела
    [SerializeField] private float lengthMul = 5f;                 // визуально длиннее
    [SerializeField] private Color aimColor = Color.red;          // цвет прицела

    /* ───── Raycast ───── */
    [Header("Raycast")]
    [SerializeField] private float shockRange = 40f;

    /* ───── аудио ───── */
    [Header("Audio")]
    [SerializeField] private AudioClip humLoop;
    [SerializeField] private AudioClip chargeWhine;
    [SerializeField] private AudioClip strikeSfx;

    /* ───── экранные эффекты (необ.) ───── */
    [Header("Screen FX (optional)")]
    [SerializeField] private CinemachineImpulseSource impulse;
    [SerializeField] private Volume chromaVolume;
    [SerializeField] private float chromaWeight = 0.7f;
    
    /* ───── collectible ───── */
    [Header("Collectibles")]
    [SerializeField] GameObject collectiblePrefab;
    [SerializeField] float dropChance = 0.3f;   // 30 %

    /* ───── runtime ───── */
    LineRenderer lr;
    AudioSource audioSrc;
    Transform player;
    float timer;
    AnimationCurve widthCurve;                                      // сохранённая кривая

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        lr = GetComponent<LineRenderer>();
        audioSrc = GetComponent<AudioSource>();

        // если материал прицела не задан – создаём простой Unlit-красный
        if (!aimMat)
            aimMat = new Material(Shader.Find("Unlit/Color")) { color = aimColor };

        widthCurve = AnimationCurve.EaseInOut(0, 0.02f, 1, baseWidth);
        lr.positionCount = 2;
        lr.enabled = false;
        lr.material = aimMat;
        lr.widthCurve = widthCurve;
    }

    void Update()
    {
        if (!player) return;

        if ((timer -= Time.deltaTime) <= 0f)
        {
            StartCoroutine(AimAndStrike());
            timer = cooldown + aimDuration + lightningTime;
        }
    }

    /* ───────── Aiming + Strike coroutine ───────── */
    IEnumerator AimAndStrike()
    {
        /* 1. Прицеливание */
        lr.enabled = true;
        lr.material = aimMat;
        lr.widthCurve = widthCurve;

        if (humLoop) { audioSrc.clip = humLoop; audioSrc.loop = true; audioSrc.Play(); }
        bool whinePlayed = false;

        Vector3 aimPt = player.position + Vector3.up * 0.8f;
        for (float t = 0; t < aimDuration; t += Time.deltaTime)
        {
            if (!whinePlayed && aimDuration - t <= 0.8f)
            {
                if (chargeWhine) audioSrc.PlayOneShot(chargeWhine);
                whinePlayed = true;
            }

            Vector3 desired = player.position + Vector3.up * 0.8f;
            aimPt = Vector3.Lerp(aimPt, desired, followLag * Time.deltaTime);

            Vector3 dir = (aimPt - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, aimPt) * lengthMul;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position + dir * dist);

            yield return null;
        }
        audioSrc.Stop();

        /* 2. Выстрел */
        lr.material = lightningMat;
        lr.widthCurve = AnimationCurve.Constant(0, 1, baseWidth * 3f);

        Vector3 dirShot = (aimPt - transform.position).normalized;
        float strikeDist = Vector3.Distance(transform.position, aimPt);
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, aimPt);

        if (strikeSfx) audioSrc.PlayOneShot(strikeSfx);
        impulse?.GenerateImpulse(0.25f);
        if (chromaVolume) chromaVolume.weight = chromaWeight;

        if (Physics.Raycast(transform.position, dirShot, out var hit, strikeDist) &&
            hit.collider.CompareTag("Player"))
        {
            hit.collider.GetComponent<PlayerDeath>()?.SpawnExplosion();
            hit.collider.GetComponent<PlayerDeath>()?.Die();
        }

        DropCollectible();

        yield return new WaitForSeconds(lightningTime);

        /* 3. Сброс */
        lr.enabled = false;
        lr.material = aimMat;
        lr.widthCurve = widthCurve;
        if (chromaVolume) chromaVolume.weight = 0f;
    }
    public void DropCollectible()
    {
        if (!collectiblePrefab) return;
        if (Random.value > dropChance) return;
        
        Vector3 spawn = transform.position + Vector3.down * 0.5f; // из-под двери
        Instantiate(collectiblePrefab, spawn, Quaternion.identity);
    }

}
