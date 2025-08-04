using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет генерацией дорожных сегментов и жёстким контролем плотности препятствий,
/// с задержкой перед первым спавном.
/// </summary>
public class WorldManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RoadMover tilePrefab;
    [SerializeField] private ObjectPool pool;

    [Header("Road & Scrolling")]
    [SerializeField, Min(2)] private int tilesOnScreen = 6;
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float speedRamp = 0.15f;
    [SerializeField] private float maxWorldSpeed = 20f;
    [SerializeField] private float despawnBackZ = 40f;

    [Header("Obstacle Settings")]
    [Tooltip("Фиксированное число препятствий на сегмент")]
    [SerializeField, Range(0, 8)] private int obstaclesPerSegment = 4;
    [Tooltip("Задержка перед первым спавном препятствий (сек)")]
    [SerializeField] private float obstacleDelaySeconds = 1f;

    private readonly List<RoadMover> tiles = new();
    private float time;

    private void Start()
    {
        RoadMover.GlobalSpeed = baseSpeed;
        SpawnInitialTiles();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (tiles.Count == 0) return;                 // ❶ защита

        time += Time.deltaTime;
        RoadMover.GlobalSpeed = Mathf.Min(
            RoadMover.GlobalSpeed + speedRamp * Time.deltaTime,
            maxWorldSpeed);

        float dz = RoadMover.GlobalSpeed * Time.deltaTime;
        foreach (var t in tiles)
            t.transform.Translate(0, 0, -dz, Space.World);

        float despawnZ = player.position.z - despawnBackZ;

        // ❷ проверяем снова, вдруг за время перемещения список стал пуст
        if (tiles.Count > 0 &&
            tiles[0].transform.position.z + tiles[0].Length * 0.5f <= despawnZ)
            RecycleFirst();
    }

    private void RecycleFirst()
    {
        if (tiles.Count == 0) return;                 // ❸ защита

        var t = tiles[0];
        tiles.RemoveAt(0);

        float newZ = tiles.Count > 0
            ? tiles[^1].transform.position.z + tiles[^1].Length
            : player.position.z;                      // fallback

        t.transform.position = new Vector3(0, 0, newZ);
        SetupSegment(t);
        tiles.Add(t);
    }


    private void SpawnInitialTiles()
    {
        var first = Instantiate(tilePrefab, transform);
        first.Init();
        first.transform.position = new Vector3(
            0, 0,
            player.position.z - first.Length * 0.5f);
        SetupSegment(first);
        tiles.Add(first);

        for (int i = 1; i < tilesOnScreen; i++)
        {
            float z = tiles[^1].transform.position.z + tiles[^1].Length;
            var mover = Instantiate(
                tilePrefab,
                new Vector3(0, 0, z),
                Quaternion.identity,
                transform);
            mover.Init();
            SetupSegment(mover);
            tiles.Add(mover);
        }
    }
    public void SetObstaclesPerSegment(int value)
    {
        obstaclesPerSegment = Mathf.Clamp(value, 0, 8);
    }

    private void SetupSegment(RoadMover mover)
    {
        var seg = mover.GetComponentInChildren<RoadSegment>(true);
        if (seg == null) return;

        // Не спавнить до истечения задержки
        int target = time < obstacleDelaySeconds ? 0 : obstaclesPerSegment;
        seg.Populate(pool, target);
    }
}
