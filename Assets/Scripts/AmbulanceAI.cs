using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Ambulance AI – минимальный поворот + уничтожение помех.
/// • Едет только по X, строя путь на 4 ближайших ряда.
/// • При столкновении с объектом с тегом "Car" вызывает Exploder и удаляет авто.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class AmbulanceAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed         = 10f;
    [SerializeField] private float waypointThreshold = 0.05f;

    [Header("Path")]
    [SerializeField, Range(1, 8)] private int rowsAhead = 4;
    [SerializeField]             private float rowStep   = 1.5f;

    private CharacterController controller;
    private readonly Queue<Transform> path = new();
    private Exploder exploder;                         // ссылка на отдельный компонент

    /* ───────── Unity ───────── */
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        exploder   = GetComponent<Exploder>();         // найдём Exploder на том же объекте
    }

    private void Update()
    {
        if (path.Count == 0) BuildPath();
        MoveAlongX();
    }

    /* ───── строим путь (как прежде) ───── */
    private void BuildPath()
    {
        path.Clear();

        var free = Object.FindObjectsByType<RoadSegment>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                         .SelectMany(s => s.GetAllFreeSlots())
                         .Where(p => p.position.z > transform.position.z)
                         .ToList();
        if (free.Count == 0) return;

        var groups = free
            .GroupBy(p => Mathf.RoundToInt(p.position.z / rowStep))
            .OrderBy(g => g.Key)
            .Take(rowsAhead)
            .ToList();
        if (groups.Count == 0) return;

        float x = transform.position.x;
        foreach (var row in groups)
        {
            var slot = row.OrderBy(p => Mathf.Abs(p.position.x - x)).First();
            path.Enqueue(slot);
            x = slot.position.x;
        }
    }

    /* ───── движение по X ───── */
    private void MoveAlongX()
    {
        if (path.Count == 0) return;

        var tgt = path.Peek();
        float dx = tgt.position.x - transform.position.x;

        if (Mathf.Abs(dx) <= waypointThreshold)
        {
            path.Dequeue();
            return;
        }

        float step = Mathf.Sign(dx) * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(step) > Mathf.Abs(dx)) step = dx;
        controller.Move(new Vector3(step, 0f, 0f));
    }

}
