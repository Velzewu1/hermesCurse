using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер игрока с поддержкой Bunny Hop:
/// • WASD: разгоном и торможением
/// • Прыжок по нажатию и автоматический прыжок при удержании после приземления
/// • Переменная высота прыжка
/// • Лёгкое гашение при посадке
/// </summary>
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerControls : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float maxGroundSpeed       = 20f;
    [SerializeField] private float acceleration         = 30f;
    [SerializeField] private float deceleration         = 35f;
    [SerializeField, Range(0f, 1f)] private float airSpeedMultiplier = 0.9f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight           = 2.2f;
    [SerializeField] private float gravity              = -18f;

    [Header("Landing Damping")]
    [SerializeField] private float landingBounceSpeed   = -2f;
    [SerializeField, Range(0f, 1f)] private float landingDampFactor = 0.4f;

    private CharacterController controller;
    private Vector2 inputRaw;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool jumpHeld;
    private bool wasGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        inputRaw = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            jumpHeld = true;
            if (controller.isGrounded)
                DoJump();
        }
        else if (ctx.canceled)
        {
            jumpHeld = false;
        }
    }

    private void Update()
    {
        bool grounded = controller.isGrounded;
        Vector3 inputDir = new Vector3(inputRaw.x, 0f, inputRaw.y).normalized;

        // Bunny Hop: автоматический прыжок при удержании после приземления
        if (jumpHeld && grounded && !wasGrounded)
        {
            DoJump();
        }

        // Горизонтальное движение
        float accel = grounded
            ? (inputDir.sqrMagnitude > 0f ? acceleration : deceleration)
            : acceleration * airSpeedMultiplier;
        float maxSpeed = grounded
            ? maxGroundSpeed
            : maxGroundSpeed * airSpeedMultiplier;

        Vector3 targetH = inputDir * maxSpeed;
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetH,
            accel * Time.deltaTime);

        // Гравитация
        verticalVelocity += gravity * Time.deltaTime;

        // Перемещение
        Vector3 move = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // Мягкая посадка
        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = Mathf.Lerp(
                verticalVelocity,
                landingBounceSpeed,
                landingDampFactor);
        }

        wasGrounded = grounded;
    }

    private void DoJump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        wasGrounded = false;
    }
}
