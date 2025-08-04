using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Поворачивает объект так, чтобы он всегда смотрел в сторону игрока.
/// </summary>
public class LookAtPlayer : MonoBehaviour
{
    [Tooltip("Transform объекта игрока. Если не указан, будет найден по тегу \"Player\".")]
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogWarning($"[{nameof(LookAtPlayer)}] Не удалось найти объект с тегом \"Player\".");
        }
    }

    private void Update()
    {
        if (playerTransform == null) 
            return;

        // Рассчитываем вектор направления к игроку, игнорируя разницу по высоте
        Vector3 direction = playerTransform.position - transform.position;
        direction.x = -90f;
        direction.z = 180f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        // Немедленная поворотка к игроку
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
