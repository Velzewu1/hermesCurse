using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    /* ───── Inspector ───── */
    [Header("Drop physics")]
    [SerializeField] float dropImpulse   = 5f;    // сила падения вниз
    [SerializeField] float settleDelay   = 0.4f;  // время полёта

    [Header("Movement")]
    [Tooltip("Множитель скорости относительно тайлов")]
    [SerializeField] float speedFactor = 1.2f;    // чуть быстрее дороги
    [Tooltip("Удалить, если позади игрока дальше этого расстояния")]
    [SerializeField] float despawnBackZ = 30f;

    [Header("Visual FX")]
    [SerializeField] float  rotationSpeed = 180f;          // °/сек
    [SerializeField] Color  glowColor     = Color.red;     // оттенок свечения
    [SerializeField] float  glowIntensity = 4f;            // HDR множитель

    /* ───── runtime ───── */
    Rigidbody   rb;
    Transform   player;
    Renderer    rend;
    bool        settled;

    /* ───── lifecycle ───── */
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        rb   = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;

        rend = GetComponentInChildren<Renderer>();
        if (rend && rend.material.HasProperty("_EmissionColor"))
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    void OnEnable()
    {
        rb.AddForce(Vector3.down * dropImpulse, ForceMode.Impulse);
        StartCoroutine(SettleRoutine());
    }

    IEnumerator SettleRoutine()
    {
        yield return new WaitForSeconds(settleDelay);

        rb.linearVelocity         = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;
        rb.useGravity      = false;
        settled            = true;
    }

    /* ───── main loop ───── */
    void Update()
    {
        // декоративное вращение всегда
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        if (!settled) return;

        float dz = RoadMover.GlobalSpeed * speedFactor * Time.deltaTime;
        transform.Translate(0, 0, -dz, Space.World);

        if (player && transform.position.z < player.position.z - despawnBackZ)
            Destroy(gameObject);
    }

    /* ───── pickup ───── */
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectManager.Instance?.AddCollectible();
            Destroy(gameObject);
        }
    }
}
