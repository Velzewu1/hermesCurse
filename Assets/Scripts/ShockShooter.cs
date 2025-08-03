using System.Collections;
using UnityEngine;

/// <summary>
/// Laser-прицел + однократный <b>смертельный</b> разряд.  
/// При попадании в игрока луч <u>моментально уничтожает</u> или выводит из строя.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ShockShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;         // ShootPoint на двери
    [SerializeField] private GameObject boltPrefab;       // визуал молнии

    [Header("Timing")]
    [SerializeField] private float aimDuration = 1.0f;
    [SerializeField] private float fireDelay   = 0.3f;
    [SerializeField] private float cooldown    = 3.0f;

    [Header("Laser visuals")]
    [SerializeField] private Color laserColor = Color.cyan;
    [SerializeField] private float laserWidth = 0.05f;

    [Header("Raycast")]
    [SerializeField] private float shockRange = 40f;

    private LineRenderer laser;
    private float        timer;
    private Transform    player;

    /* ───────── init ───────── */
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        laser = GetComponent<LineRenderer>();
        laser.positionCount = 2;
        laser.enabled  = false;
        laser.startWidth = laser.endWidth = laserWidth;
        laser.material = new Material(Shader.Find("Unlit/Color")) { color = laserColor };

        if (!firePoint)
            firePoint = transform.Find("ShootPoint") ?? transform;
    }

    private void Update()
    {
        if (!player) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            StartCoroutine(AimAndShock());
            timer = cooldown + aimDuration + fireDelay;
        }
    }

    /* ───────── coroutine ───────── */
    private IEnumerator AimAndShock()
    {
        // 1. прицел — видимый луч
        laser.enabled = true;
        float t = 0f;
        while (t < aimDuration)
        {
            Vector3 start  = firePoint.position;
            Vector3 target = player.position + Vector3.up * 0.8f;
            laser.SetPosition(0, start);
            laser.SetPosition(1, target);
            t += Time.deltaTime;
            yield return null;
        }
        laser.enabled = false;

        // 2. пауза
        yield return new WaitForSeconds(fireDelay);

        // 3. разряд — мгновенный урон
        Vector3 origin = firePoint.position;
        Vector3 dir    = (player.position + Vector3.up * 0.8f - origin).normalized;

        if (boltPrefab)
        {
            var bolt = Instantiate(boltPrefab, origin, Quaternion.identity);
            if (bolt.TryGetComponent(out LineRenderer lr))
            {
                lr.positionCount = 2;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, origin + dir * shockRange);
            }
            Destroy(bolt, 2f); // автоудаление эффекта
        }

        if (Physics.Raycast(origin, dir, out RaycastHit hit, shockRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // моментальное уничтожение игрока
                if (hit.collider.TryGetComponent(out Health hp))
                    hp.Kill();                      // ваш метод «ваншота»
                else
                    Destroy(hit.collider.gameObject); // fallback
            }
        }
    }
}
