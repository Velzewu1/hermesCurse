using System.Collections.Generic;
using System.Linq;                    // ← обязательный LINQ
using UnityEngine;

/// <summary>
/// Ambulance AI — маршрут на ближайшие <see cref="rowsAhead"/> рядов.
/// • Машина и игрок стоят на постоянной Z; движение только по X.
/// • Собирает свободные точки первых N рядов впереди (Z выше позиции машины);
/// • В каждом ряду выбирает слот по весу:
///     score = |ΔX до игрока| − kTooFar·|ΔX до себя|;
/// • Проходит точки по очереди; когда очередь пуста — строит новую.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class AmbulanceAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed         = 10f;
    [SerializeField] private float waypointThreshold = 0.05f;  // достижение по X

    [Header("Path settings")]
    [SerializeField, Range(1, 8)] private int   rowsAhead = 4;  // окно вперёд
    [SerializeField]          private float rowStep      = 1.5f;
    [SerializeField]          private float xPenalty     = 1f;  // «дальше от игрока» вес
    [SerializeField, Tooltip("Штраф за сильный уход от текущей колонки")]
    private  float kTooFar      = 0.7f;                         // 0.5–1.0

    private CharacterController controller;
    private readonly Queue<Transform> path = new();

    private void Awake() => controller = GetComponent<CharacterController>();

    private void Update()
    {
        if (path.Count == 0) BuildPath();
        MoveAlongX();
    }

    /* ───────── build path ───────── */
    private void BuildPath()
    {
        path.Clear();

        var segments = Object.FindObjectsByType<RoadSegment>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        // свободные точки только впереди (Z > мой Z)
        List<Transform> free = segments
            .SelectMany(s => s.GetAllFreeSlots())
            .Where(p => p.position.z > transform.position.z)
            .ToList();
        if (free.Count == 0) return;

        // группируем по индексу ряда
        var rowGroups = free
            .GroupBy(p => Mathf.RoundToInt(p.position.z / rowStep))
            .OrderBy(g => g.Key)          // ближний ряд → дальний
            .Take(rowsAhead)
            .ToList();
        if (rowGroups.Count == 0) return;

        foreach (var row in rowGroups)
        {
            Transform slot = row
                .OrderByDescending(p =>
                {
                    float dxPlayer = Mathf.Abs(p.position.x - player.position.x);
                    float dxSelf   = Mathf.Abs(p.position.x - transform.position.x);
                    return dxPlayer * xPenalty - kTooFar * dxSelf;
                })
                .First();

            path.Enqueue(slot);
        }

        // DEBUG: показать слоты четырёх рядов
        string msg = "[AmbulanceAI] new path:";
        foreach (var row in rowGroups)
            msg += $"\n  Z={row.First().position.z:F1} : {string.Join(", ", row.Select(p => p.name))}";
        Debug.Log(msg);
    }

    /* ───────── move X only ───────── */
    private void MoveAlongX()
    {
        if (path.Count == 0) return;

        Transform tgt = path.Peek();
        float dx = tgt.position.x - transform.position.x;

        if (Mathf.Abs(dx) <= waypointThreshold)    // достигли точки
        {
            path.Dequeue();
            return;
        }

        float step = Mathf.Sign(dx) * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(step) > Mathf.Abs(dx)) step = dx;  // не перелетаем
        controller.Move(new Vector3(step, 0f, 0f));
    }
}
