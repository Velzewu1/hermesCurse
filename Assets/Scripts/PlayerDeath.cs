using UnityEngine;

/// <summary>
/// Простая «смерть» игрока: делает персонажа невидимым, отключает управление
/// и коллизии. Используйте <c>GetComponent<PlayerDeath>().Die()</c>.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerDeath : MonoBehaviour
{
    [Tooltip("UI экран Game Over (опционально)")]
    [SerializeField] private GameObject gameOverScreen;

    private bool isDead;

    /// <summary>Вызывается внешним скриптом (например ShockShooter).</summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. отключаем управление
        var controls = GetComponent<PlayerControls>();
        if (controls) controls.enabled = false;

        // 2. скрываем все рендереры
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // 3. отключаем коллизию, чтобы не мешать дальнейшей логике
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // 4. показываем экран Game Over, если задан
        if (gameOverScreen) gameOverScreen.SetActive(true);
    }
}
