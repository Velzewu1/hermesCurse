using UnityEngine;

/// <summary>Stores tile length and exposes global scroll speed.</summary>
public class RoadMover : MonoBehaviour
{
    public static float GlobalSpeed = 8f;      // set by WorldManager
    public float Length { get; private set; }  // measured once

    public void Init()
    {
        var renders = GetComponentsInChildren<Renderer>();
        if (renders.Length == 0) { Length = 1f; return; }

        Bounds b = renders[0].bounds;
        foreach (var r in renders) b.Encapsulate(r.bounds);
        Length = b.size.z;
    }
}
