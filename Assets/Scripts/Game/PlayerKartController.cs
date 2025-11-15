using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]

public class PhysicsController : MonoBehaviour
{
    [Header("Kart Stats")]
    public float acceleration = 30f;
    public float handling = 90f;
    public float speed = 20f;

    public enum WeightClass { Light, Medium, Heavy }
    public bool canDrive = false;   // stops kart from moving until countdown is done


    [Header("Weight Settings")]
    public WeightClass weightClass = WeightClass.Medium;
    private float weightFactor;
    private float slopeFactor;
    private float handlingFactor;

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

    [Header("Collision Effects")]
    public AudioSource collisionSound;
    public ParticleSystem collisionParticles;
    public float collisionShakeIntensity = 0.15f;
    public float collisionShakeDuration = 0.3f;
    public float collisionSlowdownFactor = 0.6f;

    [Header("Boost Particle (Optional)")]
    public ParticleSystem boostParticles;

    [Header("Stabilization Settings")]
    public float distanceToGround = 0.6f;
    public float groundForce = 50f;

    // UI
    private GUIStyle weightLabelStyle;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private Vector3 camDefaultLocalPos;
    private bool isBoosting = false;
    private bool isShaking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.6f, 0f);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Weight class behavior setup
        switch (weightClass)
        {
            case WeightClass.Light:
                rb.mass = 0.8f;
                weightFactor = 0.8f;  // lighter body
                slopeFactor = 0.8f;   // slower slope response
                handlingFactor = 0.8f; // wider turns
                break;
            case WeightClass.Medium:
                rb.mass = 1.5f;
                weightFactor = 1f;
                slopeFactor = 1f;
                handlingFactor = 1f;
                break;
            case WeightClass.Heavy:
                rb.mass = 2.5f;
                weightFactor = 1.3f;  // faster top speed
                slopeFactor = 1.2f;   // stronger on slopes
                handlingFactor = 1.3f; // tighter turns
                break;
        }

        weightLabelStyle = new GUIStyle
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState { textColor = Color.white }
        };

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
        StabilizeKart();
    }

    void ApplyInput()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.LeftShift))
            turnInput *= driftMultiplier;

        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting)
            StartCoroutine(BoostCoroutine());
    }

    void UpdatePosition()
    {
        if (!canDrive)
        {
            rb.velocity = Vector3.zero;    // keeps kart still
            return;
        }

        float currentAcceleration = acceleration * (1f / weightFactor);
        if (isBoosting) currentAcceleration *= boostMultiplier;

        // --- Ground detection and slope alignment ---
        RaycastHit hit;
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 1.5f);

        if (grounded)
        {
            // Align kart with terrain slope normal
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRotation, Time.fixedDeltaTime * 8f));

            // Calculate slope direction for movement
            Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

            // Uphill / Downhill adjustment
            float slopeDot = Vector3.Dot(hit.normal, Vector3.up);
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            float slopeAdjustment = 1f;
            if (slopeAngle > 0.1f)
            {
                if (Vector3.Dot(slopeForward, Vector3.up) < 0) // Uphill
                    slopeAdjustment = 1f / slopeFactor;
                else // Downhill
                    slopeAdjustment = slopeFactor;
            }

            // Forward motion along slope
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                Vector3 force = slopeForward * moveInput * currentAcceleration * slopeAdjustment;
                rb.AddForce(force, ForceMode.Acceleration);
            }

            // Clamp speed
            float currentMaxSpeed = speed * weightFactor * (isBoosting ? boostMultiplier : 1f);
            if (rb.velocity.magnitude > currentMaxSpeed)
                rb.velocity = rb.velocity.normalized * currentMaxSpeed;

            // Turning
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                float adjustedHandling = handling * handlingFactor;
                float turnAngle = turnInput * adjustedHandling * Time.fixedDeltaTime * Mathf.Sign(moveInput);
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAngle, 0f));
            }
        }
        else
        {
            // Apply slight downward pull if airborne
            rb.AddForce(Vector3.down * groundForce * 0.5f, ForceMode.Acceleration);
        }
    }


    void HandleCamera()
    {
        if (playerCamera == null) return;

        float driftTilt = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f;
        float sideTilt = -turnInput * tiltAmount * driftTilt;

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

    void StabilizeKart()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, distanceToGround))
            rb.AddForce(Vector3.down * groundForce, ForceMode.Acceleration);
    }

    IEnumerator BoostCoroutine()
    {
        isBoosting = true;
        if (boostParticles != null) boostParticles.Play();
        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
        if (boostParticles != null) boostParticles.Stop();
    }

    // Handle collisions
    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > 2f)
        {
            // Play sound
            if (collisionSound != null)
                collisionSound.Play();

            // Particles
            if (collisionParticles != null)
                collisionParticles.Play();

            // Small speed penalty
            rb.velocity *= collisionSlowdownFactor;

            // Camera shake
            if (!isShaking && playerCamera != null)
                StartCoroutine(CameraShake());
        }
    }

    IEnumerator CameraShake()
    {
        isShaking = true;
        Vector3 originalPos = playerCamera.localPosition;

        float elapsed = 0f;
        while (elapsed < collisionShakeDuration)
        {
            float x = Random.Range(-collisionShakeIntensity, collisionShakeIntensity);
            float y = Random.Range(-collisionShakeIntensity, collisionShakeIntensity);
            playerCamera.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.localPosition = originalPos;
        isShaking = false;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 30), $"Weight Class: {weightClass}", weightLabelStyle);
    }
}
