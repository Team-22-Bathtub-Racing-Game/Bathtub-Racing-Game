using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerKartController : MonoBehaviour
{
    [Header("Kart Stats")]
    public float acceleration = 30f;
    public float handling = 90f;
    public float speed = 20f;

    public enum WeightClass { Light, Medium, Heavy }
    public WeightClass weightClass = WeightClass.Medium;

    [Header("Driving State")]
    public bool canDrive = false;

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

    [Header("Camera Shake")]
    public float collisionShakeIntensity = 0.05f;
    public float collisionShakeDuration = 0.25f;

    [Header("Engine & Drift Audio")]
    public AudioSource engineAudio;
    public AudioSource driftAudio;
    public float engineMinPitch = 0.8f;
    public float engineMaxPitch = 2.0f;
    public float engineBoostPitch = 0.3f;

    [Header("Collision Effects")]
    public AudioSource collisionSound;
    public ParticleSystem collisionParticles;
    public float collisionSlowdownFactor = 0.6f;

    [Header("Boost Particle")]
    public ParticleSystem boostParticles;

    [Header("Stabilization Settings")]
    public float groundForce = 50f;

    private Rigidbody rb;
    private Collider col;

    private float moveInput;
    private float turnInput;
    private Vector3 camDefaultLocalPos;
    private bool isBoosting = false;
    private bool isShaking = false;

    // NEW: Keeps the speed constant when W is released
    private float retainedSpeed = 0f;

    // GUI
    private GUIStyle weightLabelStyle;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.centerOfMass = new Vector3(0, -0.6f, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        switch (weightClass)
        {
            case WeightClass.Light:
                rb.mass = 0.8f;
                weightFactor = 0.8f;
                slopeFactor = 0.8f;
                handlingFactor = 0.8f;
                break;

            case WeightClass.Medium:
                rb.mass = 1.5f;
                weightFactor = 1f;
                slopeFactor = 1f;
                handlingFactor = 1f;
                break;

            case WeightClass.Heavy:
                rb.mass = 2.5f;
                weightFactor = 1.3f;
                slopeFactor = 1.2f;
                handlingFactor = 1.3f;
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
        UpdateEngineSound();
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
        {
            turnInput *= driftMultiplier;
            if (driftAudio != null && !driftAudio.isPlaying)
                driftAudio.Play();
        }
        else if (driftAudio != null && driftAudio.isPlaying)
        {
            driftAudio.Stop();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting)
            StartCoroutine(BoostCoroutine());
    }

    void UpdateEngineSound()
    {
        if (engineAudio == null) return;

        float speedPercent = rb.velocity.magnitude / (speed * weightFactor);
        engineAudio.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, speedPercent);
        engineAudio.volume = Mathf.Lerp(0.4f, 0.9f, speedPercent);

        if (isBoosting)
            engineAudio.pitch += engineBoostPitch;
    }

    void UpdatePosition()
    {
        if (!canDrive)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        float currentAcceleration = acceleration * (1f / weightFactor);
        if (isBoosting) currentAcceleration *= boostMultiplier;

        float halfHeight = col != null ? col.bounds.extents.y : 1f;
        Vector3 rayStart = col != null ? col.bounds.center + Vector3.up * 0.1f
                                       : transform.position + Vector3.up * 0.1f;
        float rayLength = halfHeight + 0.5f;

        bool grounded = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength);

        if (grounded)
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRotation, Time.fixedDeltaTime * 10f));

            Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

            // ---------------- CONSTANT SPEED SYSTEM (GROUND) ----------------
            if (moveInput > 0.1f)
            {
                Vector3 force = slopeForward * currentAcceleration * slopeFactor;
                rb.AddForce(force, ForceMode.Acceleration);
                retainedSpeed = rb.velocity.magnitude;
            }
            else if (moveInput < -0.1f)
            {
                Vector3 force = slopeForward * moveInput * currentAcceleration * slopeFactor;
                rb.AddForce(force, ForceMode.Acceleration);
                retainedSpeed = rb.velocity.magnitude;
            }
            else
            {
                if (rb.velocity.magnitude < retainedSpeed)
                    rb.velocity = rb.velocity.normalized * retainedSpeed;
            }
        }
        else
        {
            // ---------------- CONSTANT SPEED SYSTEM (AIR) ----------------
            if (moveInput > 0.1f)
            {
                Vector3 force = transform.forward * currentAcceleration;
                rb.AddForce(force, ForceMode.Acceleration);
                retainedSpeed = rb.velocity.magnitude;
            }
            else if (moveInput < -0.1f)
            {
                Vector3 force = transform.forward * moveInput * currentAcceleration;
                rb.AddForce(force, ForceMode.Acceleration);
                retainedSpeed = rb.velocity.magnitude;
            }
            else
            {
                if (rb.velocity.magnitude < retainedSpeed)
                    rb.velocity = rb.velocity.normalized * retainedSpeed;
            }
        }

        // Clamp speed
        float currentMaxSpeed = speed * weightFactor * (isBoosting ? boostMultiplier : 1f);
        if (rb.velocity.magnitude > currentMaxSpeed)
            rb.velocity = rb.velocity.normalized * currentMaxSpeed;

        // Turning
        float adjustedHandling = handling * handlingFactor;
        float turnAngle = turnInput * adjustedHandling * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnAngle, 0));

        // Velocity Fix
        if (rb.velocity.magnitude > 0.1f)
        {
            rb.velocity = transform.forward * rb.velocity.magnitude;
        }


    }

    void HandleCamera()
    {
        if (playerCamera == null) return;

        float driftTilt = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f;
        float sideTilt = -turnInput * tiltAmount * driftTilt;

        playerCamera.localRotation = Quaternion.Slerp(
            playerCamera.localRotation,
            Quaternion.Euler(0, 0, sideTilt),
            Time.deltaTime * tiltSpeed
        );

        float speedVal = rb.velocity.magnitude;
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount * Mathf.Clamp01(speedVal / speed);

        playerCamera.localPosition = Vector3.Lerp(
            playerCamera.localPosition,
            camDefaultLocalPos + new Vector3(0, bobOffset, 0),
            Time.deltaTime * 10f
        );
    }

    void StabilizeKart()
    {
        float halfHeight = col != null ? col.bounds.extents.y : 1f;
        Vector3 rayStart = col != null ? col.bounds.center : transform.position;
        float rayLength = halfHeight + 0.2f;

        if (!Physics.Raycast(rayStart, Vector3.down, rayLength))
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Road") ||
            collision.collider.CompareTag("Terrain"))
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > 2f)
        {
            if (collisionSound != null)
                collisionSound.Play();

            if (collisionParticles != null)
                collisionParticles.Play();

            rb.velocity *= collisionSlowdownFactor;

            if (!isShaking && playerCamera != null)
                StartCoroutine(CameraShake());
        }
    }

    IEnumerator CameraShake()
    {
        isShaking = true;

        Vector3 originalPos = playerCamera.localPosition;
        float time = 0f;

        while (time < collisionShakeDuration)
        {
            float x = Random.Range(-collisionShakeIntensity, collisionShakeIntensity);
            float y = Random.Range(-collisionShakeIntensity, collisionShakeIntensity);

            playerCamera.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            time += Time.deltaTime;
            yield return null;
        }

        playerCamera.localPosition = originalPos;
        isShaking = false;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 30),
            $"Weight Class: {weightClass}",
            weightLabelStyle);
    }
}


