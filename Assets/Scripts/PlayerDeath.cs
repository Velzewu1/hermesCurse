using System.Collections;
using UnityEngine;  

/// <summary>
/// Простая «смерть» игрока: делает персонажа невидимым, отключает управление
/// и коллизии. Используйте <c>GetComponent<PlayerDeath>().Die()</c>.
/// Вместо вызова Die() напрямую, учитывается флаг IsInvulnerable.
/// Также можно спавнить взрыв и быстро сбросить игрока вниз.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerDeath : MonoBehaviour
{
    [Tooltip("UI экран Game Over (опционально)")]
    [SerializeField] private GameObject gameOverScreen;

    [Header("Death Effects")]
    [Tooltip("Префаб эффекта взрыва (опционально)")]
    [SerializeField] private GameObject explosionPrefab;

    private bool isDead;
    /// <summary>Игрок неуязвим, Die() не срабатывает.</summary>
    public static bool IsInvulnerable { get; set; }
    private WorldManager worldManager;

    /// <summary>Вызывается внешним скриптом (например ShockShooter).</summary>
    private void StopWorld()
    {
        worldManager = GameObject.FindAnyObjectByType<WorldManager>();
        worldManager.baseSpeed = 0f;
        worldManager.maxWorldSpeed = 0f;
        worldManager.speedRamp = 0;
    }
    public void Die()
    {
        
        if (isDead || IsInvulnerable) return;
        isDead = true;
        
        StopWorld();

        // 1. отключаем управление
        var controls = GetComponent<PlayerControls>();
        if (controls) controls.enabled = false;

        // 2. скрываем все рендереры
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // 3. отключаем коллизию и контроллер
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // 4. показываем экран Game Over, если задан
        if (gameOverScreen) gameOverScreen.SetActive(true);
    }

    /// <summary>
    /// Спавнит эффект взрыва на текущей позиции.
    /// </summary>
    public void SpawnExplosion()
    {
        Vector3 blowPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.5f);
        if (explosionPrefab)
            Instantiate(explosionPrefab, blowPos, Quaternion.identity);
    }

    /// <summary>
    /// Мгновенно спускает игрока вниз и вызывает Die().
    /// </summary>
    public void FallAndDie()
    {
        if (isDead) return;
        StopWorld();
        // запускаем плавное падение
        StartCoroutine(FallCoroutine());
    }
    private IEnumerator FallCoroutine()
    {       
        // параметры падения
        float fallDistance = 3f;
        float duration = 2f; // скорость падения

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * fallDistance;
        float t = 0f;

        // анимация плавного падения
        while (t < duration)
        {
            t += Time.deltaTime;
            float frac = t / duration;
            transform.position = Vector3.Lerp(startPos, endPos, frac);
            yield return null;
        }

        // завершаем местоположение и вызываем эффект взрыва + смерть
        transform.position = endPos;
        Die();
    }
}
