using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float followDelay = 5f; // чем больше — тем "тяжелее" камера
    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        // Плавное движение к цели с запаздыванием
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, followDelay);

        // Смотрим на персонажа
        transform.LookAt(target.position + Vector3.up * 1.5f); // можно сместить взгляд немного вверх
    }
}
