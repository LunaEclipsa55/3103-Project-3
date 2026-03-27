using UnityEngine;
using UnityEngine.InputSystem;

public class Move3D : MonoBehaviour
{
    [SerializeField] float MoveSpeed = 5f;
    [SerializeField] float JumpForce = 7f;
    [SerializeField] private Transform FeetPosition;
    [SerializeField] private float GroundCheckRadius = 0.1f;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private float FallMultiplier = 2f;
    [SerializeField] private float JumpCD = 0.1f;

    private Rigidbody rb;
    private bool isGrounded;
    private float jumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        CheckGround();

        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            Jump();

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (FallMultiplier - 1f) * Time.deltaTime;
        }

        Move();
    }

    void Move()
    {
        float input = 0f;
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.aKey.isPressed) input -= 1f;
            if (k.dKey.isPressed) input += 1f;
        }
        input = Mathf.Clamp(input, -1f, 1f);

        float target = input * MoveSpeed;
        float current = rb.linearVelocity.x;
        float accel = 25f;
        float decel = 35f;
        float rate = (Mathf.Abs(target) > Mathf.Abs(current)) ? accel : decel;

        float newX = Mathf.MoveTowards(current, target, rate * Time.deltaTime);

        Vector3 v = rb.linearVelocity;
        v.x = newX;
        rb.linearVelocity = v;

        if (Mathf.Abs(input) > 0.001f)
        {
            float targetY = (input > 0f) ? 0f : 180f;
            Quaternion tgt = Quaternion.Euler(0f, targetY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, tgt, 12f * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = JumpForce;
            rb.linearVelocity = velocity;
            isGrounded = false;
        }
    }

    void CheckGround()
    {
        if (jumpTimer < JumpCD)
        {
            jumpTimer += Time.deltaTime;
            return;
        }

        isGrounded = FeetPosition != null &&
                     Physics.CheckSphere(FeetPosition.position, GroundCheckRadius, GroundLayer);

        if (isGrounded)
            jumpTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (FeetPosition == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(FeetPosition.position, GroundCheckRadius);
    }
}