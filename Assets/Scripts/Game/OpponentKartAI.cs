using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OpponentKartAI : MonoBehaviour
{
    [Header("AI Movement Settings")]
    public float acceleration = 20f;
    public float maxSpeed = 15f;

    [Header("Slope Settings (match player)")]
    public float slopeFactor = 1f;
    public float groundForce = 50f;   

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        HandleSlopeMovement();
    }

    void HandleSlopeMovement()
    {
        // Detect the terrain below the kart
        RaycastHit hit;
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 1.5f);

        if (grounded)
        {
            Quaternion slopeRotation =
                Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRotation, Time.fixedDeltaTime * 8f));

            Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

            // Detect uphill/downhill
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            float adjustment = 1f;

            if (slopeAngle > 0.1f)
            {
                // Uphill (slow down)
                if (Vector3.Dot(slopeForward, Vector3.up) < 0)
                    adjustment = 1f / slopeFactor;
                else
                    adjustment = slopeFactor; // Downhill (speed up)
            }

            Vector3 force = slopeForward * acceleration * adjustment;
            rb.AddForce(force, ForceMode.Acceleration);

            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        else
        {
            rb.AddForce(Vector3.down * groundForce * 0.5f, ForceMode.Acceleration);
        }
    }
}
