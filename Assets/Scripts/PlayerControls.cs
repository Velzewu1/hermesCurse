using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerControls : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float maxGroundSpeed = 20f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField, Range(0f, 1f)] private float airSpeedMultiplier = 0.9f;

    [Header("Jump & Gravity")]
    public float jumpHeight           = 2.2f;
    public float gravity              = -18f;

    [Header("Landing Damping")]
    [SerializeField] private float landingBounceSpeed = -2f;
    [SerializeField, Range(0f, 1f)] private float landingDampFactor = 0.4f;

    private CharacterController controller;
    private Animator anim; // ← добавлено

    private Vector2 inputRaw;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool jumpHeld;
    private bool wasGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>(); // ← ищем Animator в дочерних объектах
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

        // АНИМАЦИЯ: передвигаем isGrounded
        if (anim)
            anim.SetBool("isGrounded", grounded);

        Vector3 inputDir = new Vector3(inputRaw.x, 0f, inputRaw.y).normalized;

        if (jumpHeld && grounded && !wasGrounded)
        {
            DoJump();
        }

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

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);

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
