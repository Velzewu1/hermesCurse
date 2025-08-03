using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Спавнит фиксированное число препятствий на сегмент,
/// равномерно распределяя их по рядам (не более двух в ряд).
/// </summary>
public class RoadSegment : MonoBehaviour
{
    [SerializeField] private Transform spawnRoot;           // “SpawnPoints”

    private readonly List<List<Transform>>  rows         = new();
    private readonly List<List<GameObject>> activePerRow = new();
    private ObjectPool pool;

    /* ───────── initialization ───────── */

    private void Awake()
    {
        spawnRoot = spawnRoot ? spawnRoot
                              : transform.Find("SpawnPoints") ?? transform;

        foreach (Transform row in spawnRoot)
        {
            var slots  = new List<Transform>();
            var active = new List<GameObject>();

            foreach (Transform slot in row)
            {
                slots.Add(slot);
                active.Add(null);
            }
            rows.Add(slots);
            activePerRow.Add(active);
        }
    }

    /* ───────── main spawn ───────── */

    public void Populate(ObjectPool sharedPool, int targetPerSegment)
    {
        pool = sharedPool;
        int rowCount = rows.Count;
        if (rowCount == 0) return;

        /* 1. очистка старых объектов — ❶ Unity-safe проверка */
        for (int r = 0; r < rowCount; r++)
        {
            for (int i = 0; i < activePerRow[r].Count; i++)
            {
                GameObject go = activePerRow[r][i];
                if (go) pool.Release(go);          // only if «живой»
                activePerRow[r][i] = null;
            }
        }

        /* 2. распределение нового количества */
        int baseCount = targetPerSegment / rowCount;
        int extra     = targetPerSegment % rowCount;

        for (int r = 0; r < rowCount; r++)
        {
            int toSpawn   = Mathf.Min(baseCount + (r < extra ? 1 : 0), 2);
            int slotCount = rows[r].Count;
            if (slotCount == 0) continue;

            for (int spawned = 0; spawned < toSpawn; )
            {
                int idx = Random.Range(0, slotCount);

                // ❷ слот мог «исчезнуть» вместе с тайлом
                Transform slot = rows[r][idx];
                if (slot == null || activePerRow[r][idx] != null) continue;

                GameObject obst = pool.GetRandomObstacle();
                if (!obst) break;

                obst.transform.SetParent(slot, true);
                obst.transform.localPosition = Vector3.zero;
                obst.transform.localRotation = Quaternion.identity;

                activePerRow[r][idx] = obst;
                spawned++;
            }
        }
    }

    /* ───────── public API ───────── */

    /// <summary>Все свободные spawn-point’ы сегмента.</summary>
    public List<Transform> GetAllFreeSlots()
    {
        var free = new List<Transform>();
        for (int r = 0; r < rows.Count; r++)
            for (int i = 0; i < rows[r].Count; i++)
                if (activePerRow[r][i] == null)
                    free.Add(rows[r][i]);
        return free;
    }

    public bool IsSlotOccupied(Transform slot)
    {
        for (int r = 0; r < rows.Count; r++)
        {
            int idx = rows[r].IndexOf(slot);
            if (idx >= 0) return activePerRow[r][idx] != null;
        }
        return false;
    }

    public IEnumerable<Transform> AllSpawnPoints
    {
        get
        {
            foreach (var row in rows)
                foreach (var slot in row)
                    yield return slot;
        }
    }
}
