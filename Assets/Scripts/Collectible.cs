using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Header("Drop physics")]
    [SerializeField] private float dropImpulse = 5f;    // сила падения вниз
    [SerializeField] private float settleDelay = 0.4f;  // время полёта

    [Header("Movement")]
    [Tooltip("Множитель скорости относительно тайлов")]
    [SerializeField] private float speedFactor = 1.2f;  
    [Tooltip("Удалить, если позади игрока дальше этого расстояния")]
    [SerializeField] private float despawnBackZ = 30f;

    [Header("Visual FX")]
    [SerializeField] private float rotationSpeed = 180f; // °/сек

    private Rigidbody rb;
    private Transform player;
    private bool settled;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;

        // Настраиваем Rigidbody
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        // Бросок вниз и начало посадки
        rb.AddForce(Vector3.down * dropImpulse, ForceMode.Impulse);
        StartCoroutine(SettleRoutine());
    }

    private IEnumerator SettleRoutine()
    {
        yield return new WaitForSeconds(settleDelay);

        // Останавливаем физику
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        settled = true;
    }

    private void Update()
    {
        // Постоянное вращение
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        if (!settled) return;

        // Движение вместе с дорожкой
        float dz = RoadMover.GlobalSpeed * speedFactor * Time.deltaTime;
        transform.Translate(0f, 0f, -dz, Space.World);

        // Самоуничтожение, если слишком позади
        if (player != null && transform.position.z < player.position.z - despawnBackZ)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectManager.Instance?.AddCollectible();
            Destroy(gameObject);
        }
    }
}
