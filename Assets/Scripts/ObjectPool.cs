using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Obstacle prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Collectible prefabs")]
    [SerializeField] private GameObject[] collectiblePrefabs;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools   = new();
    private readonly Dictionary<GameObject, GameObject>        reverse = new();

    /* ─── public API ─── */
    public GameObject GetRandomObstacle()    => GetRandomFrom(obstaclePrefabs);
    public GameObject GetRandomCollectible() => GetRandomFrom(collectiblePrefabs);
    public void       Release(GameObject o)  { if (reverse.TryGetValue(o, out var p)) { o.SetActive(false); pools[p].Enqueue(o); } else Destroy(o); }

    /* ─── internal ─── */
    private GameObject GetRandomFrom(GameObject[] list)
    {
        if (list.Length == 0) return null;
        return Get(list[Random.Range(0, list.Length)]);
    }
    private GameObject Get(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var q))
            pools[prefab] = q = new Queue<GameObject>();

        if (q.Count == 0)
        {
            var inst = Instantiate(prefab, transform);
            inst.SetActive(false);
            reverse[inst] = prefab;
            q.Enqueue(inst);
        }

        var obj = q.Dequeue();
        obj.SetActive(true);

        // --- Поворачиваем объект, если он из obstaclePrefabs ---
        if (System.Array.Exists(obstaclePrefabs, p => p == prefab))
        {
            obj.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // по Y
        }
        else
        {
            obj.transform.rotation = Quaternion.identity; // нормальный поворот для других
        }

        return obj;
    }

}
