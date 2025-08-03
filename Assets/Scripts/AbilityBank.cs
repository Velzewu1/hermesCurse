using System;
using UnityEngine;

/// <summary>
/// Simply routes ability‐spend requests into the CollectManager bar.
/// Phase 1 & 2 require the bar to be full (then empties it).
/// Phase 3 costs exactly 1 collectible.
/// </summary>
public class AbilityBank : MonoBehaviour
{
    public static AbilityBank Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Try to spend for a given phase:
    /// • phase 1 or 2: require full bar, then bar→0  
    /// • phase 3: cost 1  
    /// Returns true on success.
    /// </summary>
    public bool TrySpend(int phase)
    {
        var cm = CollectManager.Instance;
        if (cm == null) return false;

        switch (phase)
        {
            case 1:
            case 2:
                if (!cm.IsFull) return false;
                return cm.UseCollectible(cm.MaxCount);

            case 3:
                return cm.UseCollectible(1);

            default:
                Debug.LogWarning($"AbilityBank: invalid phase {phase}");
                return false;
        }
    }
}
