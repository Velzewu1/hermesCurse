using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls building tiles that scroll with the road and recycle endlessly.
/// </summary>
public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RoadMover buildingPrefab; // Префаб здания с RoadMover
    [SerializeField] private int buildingsOnScreen = 6;
    [SerializeField] private float spawnOffsetX = 10f; // Смещение от дороги по X
    [SerializeField] private float despawnBackZ = 40f;

    private readonly List<RoadMover> buildings = new();

    private void Start()
    {
        SpawnInitialBuildings();
    }

    private void Update()
    {
        float dz = RoadMover.GlobalSpeed * Time.deltaTime;

        foreach (var b in buildings)
            b.transform.Translate(0, 0, -dz, Space.World);

        float despawnZ = player.position.z - despawnBackZ;

        if (buildings.Count > 0 &&
            buildings[0].transform.position.z + buildings[0].Length * 0.5f <= despawnZ)
        {
            RecycleFirst();
        }
    }

    private void SpawnInitialBuildings()
    {
        // Первое здание
        var first = Instantiate(buildingPrefab, transform);
        first.Init();
        first.transform.position = new Vector3(spawnOffsetX, 0, player.position.z - first.Length * 0.5f);
        buildings.Add(first);

        // Остальные по цепочке
        for (int i = 1; i < buildingsOnScreen; i++)
        {
            float z = buildings[^1].transform.position.z + buildings[^1].Length;
            var next = Instantiate(buildingPrefab, new Vector3(spawnOffsetX, 0, z), Quaternion.identity, transform);
            next.Init();
            buildings.Add(next);
        }
    }

    private void RecycleFirst()
    {
        var b = buildings[0];
        buildings.RemoveAt(0);

        float newZ = buildings[^1].transform.position.z + buildings[^1].Length;
        b.transform.position = new Vector3(spawnOffsetX, 0, newZ);

        buildings.Add(b);
    }
}
