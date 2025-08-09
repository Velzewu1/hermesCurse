using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет генерацией дорожных сегментов и жёстким контролем плотности препятствий,
/// с задержкой перед первым спавном в виде зоны близко к игроку (по Z).
/// </summary>
public class WorldManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RoadMover tilePrefab;
    [SerializeField] private ObjectPool pool;

    [Header("Road & Scrolling")]
    [SerializeField, Min(2)] private int tilesOnScreen   = 6;
    [SerializeField]          public float baseSpeed      = 8f;
    [SerializeField]          public float speedRamp      = 0.15f;
    [SerializeField]          public float maxWorldSpeed  = 20f;
    [SerializeField] private float despawnBackZ          = 40f;

    [Header("Obstacle Settings")]
    [Tooltip("Фиксированное число препятствий на сегмент")]
    [SerializeField, Range(0, 8)] private int obstaclesPerSegment = 4;
    [Tooltip("Расстояние по Z от игрока, в пределах которого препятствия не спавнятся")]
    [SerializeField] private float obstacleDelayDistance = 10f;

    private readonly List<RoadMover> tiles = new();

    private void Start()
    {
        RoadMover.GlobalSpeed = baseSpeed;
        SpawnInitialTiles();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (tiles.Count == 0) return;

        // Рамп скорости мира
        RoadMover.GlobalSpeed = Mathf.Min(
            RoadMover.GlobalSpeed + speedRamp * Time.deltaTime,
            maxWorldSpeed);

        float dz = RoadMover.GlobalSpeed * Time.deltaTime;
        foreach (var t in tiles)
            t.transform.Translate(0, 0, -dz, Space.World);

        float despawnZ = player.position.z - despawnBackZ;
        if (tiles[0].transform.position.z + tiles[0].Length * 0.5f <= despawnZ)
            RecycleFirst();
    }

    private void SpawnInitialTiles()
    {
        // Первый тайл позиционируем прямо под игроком
        var first = Instantiate(tilePrefab, transform);
        first.Init();
        first.transform.position = new Vector3(
            0, 0,
            player.position.z - first.Length * 0.5f);
        SetupSegment(first);
        tiles.Add(first);

        // Остальные тайлы в конец
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

    private void RecycleFirst()
    {
        var mover = tiles[0];
        tiles.RemoveAt(0);

        // Сдвигаем за последний
        float newZ = tiles.Count > 0
            ? tiles[^1].transform.position.z + tiles[^1].Length
            : player.position.z;
        mover.transform.position = new Vector3(0, 0, newZ);

        SetupSegment(mover);
        tiles.Add(mover);
    }

    /// <summary>
    /// Спавнит в сегмент ровно obstaclesPerSegment препятствий,
    /// но только если сегмент находится дальше obstacleDelayDistance от игрока.
    /// </summary>
    private void SetupSegment(RoadMover mover)
    {
        var seg = mover.GetComponentInChildren<RoadSegment>(true);
        if (seg == null) return;

        // Вычисляем дистанцию по Z между серединой сегмента и игроком
        float segmentCenterZ = mover.transform.position.z;
        float playerZ       = player.position.z;
        float distance      = segmentCenterZ - playerZ;

        int target = distance < obstacleDelayDistance
            ? 0
            : obstaclesPerSegment;

        seg.Populate(pool, target);
    }

    /// <summary>
    /// Публичный метод для динамического изменения числа препятствий.
    /// </summary>
    public void SetObstaclesPerSegment(int value)
    {
        obstaclesPerSegment = Mathf.Clamp(value, 0, 8);
    }
}
