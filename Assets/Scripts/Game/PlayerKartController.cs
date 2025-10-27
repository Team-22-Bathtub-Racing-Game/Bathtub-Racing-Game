using UnityEngine;
using System.Collections;

/// <summary>
/// Physics & Collisions
/// PhysicsController: Uses Unity’s physics engine to apply forces and detect collisions
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PhysicsController : MonoBehaviour
{
    [Header("Kart Stats")]
    public float acceleration = 30f;
    public float handling = 90f;
    public float speed = 20f;

    public enum WeightClass { Light, Medium, Heavy }

    [Header("Weight Settings")]
    public WeightClass weightClass = WeightClass.Medium;
    private float weightFactor;

    [Header("Drift & Boost")]
    public float driftMultiplier = 1.5f;
    public float boostMultiplier = 1.5f;
    public float boostDuration = 3f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float tiltAmount = 5f;
    public float tiltSpeed = 5f;
    public float bobAmount = 0.05f;
    public float bobSpeed = 4f;

    [Header("Kart Lean")]
    public Transform Kart;
    public float leanAmount = 10f;
    public float leanSpeed = 5f;

    [Header("Boost Particle (Optional)")]
    public ParticleSystem boostParticles;

    [Header("Stabilization Settings")]
    public float distanceToGround = 0.6f;  // half of kart height
    public float groundForce = 50f;        // downward stabilizing force

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private Vector3 camDefaultLocalPos;
    private bool isBoosting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.6f, 0f);  // lower for stability
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Apply weight settings
        switch (weightClass)
        {
            case WeightClass.Light:
                rb.mass = 0.8f;
                weightFactor = 0.7f; // wider turns, faster acceleration
                break;
            case WeightClass.Medium:
                rb.mass = 1.5f;
                weightFactor = 1f; // balanced
                break;
            case WeightClass.Heavy:
                rb.mass = 2.5f;
                weightFactor = 1.4f; // tighter turns, slower acceleration
                break;
        }

        if (playerCamera != null)
        {
            camDefaultLocalPos = playerCamera.localPosition;
            playerCamera.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        ApplyInput();
    }

    void FixedUpdate()
    {
        UpdatePosition();
        HandleCamera();
        HandleKartLean();
        StabilizeKart();
    }

    /// <summary>
    /// Apply player input to the kart
    /// </summary>
    void ApplyInput()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Drift input
        if (Input.GetKey(KeyCode.LeftShift))
            turnInput *= driftMultiplier;

        // Boost input
        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting)
            StartCoroutine(BoostCoroutine());
    }

    /// <summary>
    /// Apply physics forces to move the kart
    /// </summary>
    void UpdatePosition()
    {
        float currentAcceleration = acceleration * (1f / weightFactor);
        if (isBoosting) currentAcceleration *= boostMultiplier;

        // Forward/backward motion
        if (Mathf.Abs(moveInput) > 0.01f)
            ApplyForce(Kart, transform.forward * moveInput * currentAcceleration);

        // Clamp speed
        float currentMaxSpeed = ComputeTopSpeed() * (isBoosting ? boostMultiplier : 1f);
        if (rb.velocity.magnitude > currentMaxSpeed)
            rb.velocity = rb.velocity.normalized * currentMaxSpeed;

        // Turning
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            // Wider turns for lighter karts
            float adjustedHandling = handling / weightFactor;
            float turnAngle = turnInput * adjustedHandling * Time.fixedDeltaTime * Mathf.Sign(moveInput);
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAngle, 0f));
        }
    }

    /// <summary>
    /// Applies a force to the kart using Unity physics
    /// </summary>
    void ApplyForce(Transform Kart, Vector3 force)
    {
        rb.AddForce(force, ForceMode.Acceleration);
    }

    /// <summary>
    /// Camera tilt and bobbing effects
    /// </summary>
    void HandleCamera()
    {
        if (playerCamera == null) return;

        float driftMultiplier = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f;
        float sideTilt = -turnInput * tiltAmount * driftMultiplier;

        playerCamera.localRotation = Quaternion.Slerp(
            playerCamera.localRotation,
            Quaternion.Euler(0f, 0f, sideTilt),
            Time.deltaTime * tiltSpeed
        );

        float speedVal = rb.velocity.magnitude;
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount * Mathf.Clamp01(speedVal / speed);
        Vector3 basePos = camDefaultLocalPos + new Vector3(0f, 0.6f, -0.3f);
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, basePos + new Vector3(0f, bobOffset, 0f), Time.deltaTime * 10f);
    }

    /// <summary>
    /// Kart model lean when turning
    /// </summary>
    void HandleKartLean()
    {
        if (Kart == null) return;

        float targetLean = -turnInput * leanAmount;
        Kart.localRotation = Quaternion.Slerp(Kart.localRotation, Quaternion.Euler(0f, 0f, targetLean), Time.deltaTime * leanSpeed);
    }

    /// <summary>
    /// Stabilizes the kart to prevent it from floating or flipping during collisions
    /// </summary>
    void StabilizeKart()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, distanceToGround))
        {
            rb.AddForce(Vector3.down * groundForce, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Boost coroutine
    /// </summary>
    IEnumerator BoostCoroutine()
    {
        isBoosting = true;
        if (boostParticles != null) boostParticles.Play();
        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
        if (boostParticles != null) boostParticles.Stop();
    }

    float ComputeTopSpeed() => speed;
    float ComputeHandling() => handling;
}
