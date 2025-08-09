using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class ShockShooter : MonoBehaviour
{
    [Header("Beam")]
    [Tooltip("Материал LineRenderer (назначается в инспекторе)")]
    [SerializeField] private Material beamMaterial;

    [Header("Aim Settings")]
    [Tooltip("Максимальная ширина прицела")]
    [SerializeField] private float baseWidth   = 0.12f;
    [SerializeField] private float lengthMul   = 5f;

    [Header("Timing (s)")]
    [SerializeField] private float aimDuration   = 4f;
    [SerializeField] private float followLag     = 3f;
    [SerializeField] private float lightningTime = 0.45f;
    [SerializeField] private float cooldown      = 4f;

    [Header("Raycast")]
    [SerializeField] private float shockRange    = 40f;

    [Header("Audio")]
    [Tooltip("Звук ваншота")]
    [SerializeField] private AudioClip strikeSfx;

    [Header("Screen FX (optional)")]
    [SerializeField] private CinemachineImpulseSource impulse;

    [Header("Collectibles")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField, Range(0f,1f)] private float dropChance = 0.3f;

    // runtime  
    private LineRenderer    lr;
    private AudioSource     audioSrc;
    private Transform       player;
    private float           timer;
    private AnimationCurve  widthCurve;

    private void Awake()
    {
        player   = GameObject.FindWithTag("Player")?.transform;
        lr       = GetComponent<LineRenderer>();
        audioSrc = GetComponent<AudioSource>();

        if (beamMaterial == null)
            Debug.LogError("ShockShooter: beamMaterial not set!", this);

        lr.material       = beamMaterial;
        widthCurve        = AnimationCurve.EaseInOut(0, 0, 1, baseWidth);
        lr.positionCount  = 2;
        lr.enabled        = false;
        lr.widthCurve     = widthCurve;
    }

    private void Update()
    {
        if (player == null) return;

        if ((timer -= Time.deltaTime) <= 0f)
        {
            StartCoroutine(AimAndStrike());
            timer = cooldown + aimDuration + lightningTime;
        }
    }

    private IEnumerator AimAndStrike()
    {
        // 1) Прицел — просто показываем beam, меняем ширину по кривой
        lr.enabled     = true;
        lr.widthCurve  = widthCurve;

        Vector3 aimPt = player.position + Vector3.up * 0.8f;
        for (float t = 0; t < aimDuration; t += Time.deltaTime)
        {
            Vector3 desired = player.position + Vector3.up * 0.8f;
            aimPt = Vector3.Lerp(aimPt, desired, followLag * Time.deltaTime);

            Vector3 dir  = (aimPt - transform.position).normalized;
            float   dist = Vector3.Distance(transform.position, aimPt) * lengthMul;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position + dir * dist);

            yield return null;
        }

        // 2) Выстрел — делаем шире в 3×
        lr.widthCurve = AnimationCurve.Constant(0, 1, baseWidth * 3f);

        Vector3 dirShot    = (aimPt - transform.position).normalized;
        float   strikeDist = Vector3.Distance(transform.position, aimPt);
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, aimPt);

        if (strikeSfx != null)
            audioSrc.PlayOneShot(strikeSfx);

        impulse?.GenerateImpulse();

        if (Physics.Raycast(transform.position, dirShot, out var hit, strikeDist) &&
            hit.collider.CompareTag("Player"))
        {
            var pd = hit.collider.GetComponent<PlayerDeath>();
            pd?.SpawnExplosion();
            pd?.Die();
        }

        DropCollectible();

        yield return new WaitForSeconds(lightningTime);

        // 3) Скрываем beam и сбрасываем ширину
        lr.enabled     = false;
        lr.widthCurve  = widthCurve;
    }

    public void DropCollectible()
    {
        if (collectiblePrefab == null) return;
        if (Random.value > dropChance) return;

        Vector3 spawn = transform.position + Vector3.down * 0.5f;
        Instantiate(collectiblePrefab, spawn, Quaternion.identity);
    }
}
