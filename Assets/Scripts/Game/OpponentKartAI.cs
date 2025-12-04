using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;

[RequireComponent(typeof(Rigidbody))]
public class OpponentKartAI : MonoBehaviour
{
    [Header("Waypoint initizer")]
    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;
    public int currentWaypoint;
    public float waypointRange = 15f;

    [Header("AI Movement Settings")]
    public float acceleration = 20f;
    public float maxSpeed = 15f;

    [Header("Slope Settings (match player)")]
    public float slopeFactor = 1f;
    public float groundForce = 50f;

    private Rigidbody rb;

    private float brakingZoneSpeed = -1f;

    [Header("Race Control")]
    public bool canDrive = false;

    private RacerInfo racerInfo;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;


        waypoints = waypointContainer.waypoints;
        currentWaypoint = 0;

        currentWaypoint++;
        if (currentWaypoint >= waypoints.Count)
        {
            currentWaypoint = 0;

            if (!racerInfo.hasFinished)
                racerInfo.currentLap++;
        }

        racerInfo = GetComponent<RacerInfo>();
        racerInfo.totalLaps = PlayerPrefs.GetInt("SelectedLapCount", 3);
    }

    void FixedUpdate()
    {
        if (!canDrive)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if (waypoints != null && waypoints.Count > 0)
        {
            HandleWaypointMovement();
        }
        else
        {
            HandleSlopeMovement();
        }

        // Sync AI progress to RacerInfo
        if (racerInfo != null)
        {
            racerInfo.currentWaypoint = currentWaypoint;
            racerInfo.distanceToNext =
                Vector3.Distance(transform.position, waypoints[currentWaypoint].position);
        }

    }

    void HandleWaypointMovement()
    {
        Transform targetWaypoint = waypoints[currentWaypoint];
        Vector3 targetPos = targetWaypoint.position;

        RaycastHit hit;
        bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 1.5f);

        Vector3 desiredDireection;


        if (grounded)
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRotation, Time.fixedDeltaTime * 8f));

            Vector3 slopeFoward = Vector3.ProjectOnPlane((targetPos - transform.position).normalized, hit.normal).normalized;
            desiredDireection = slopeFoward;
        }
        else
        {
            desiredDireection = (targetPos - transform.position).normalized;
        }

        if (desiredDireection.sqrMagnitude > 0.01f && grounded)
        {
            Quaternion directionalRotation = Quaternion.LookRotation(desiredDireection, hit.normal);

            Quaternion finalRotation = Quaternion.Slerp(rb.rotation, directionalRotation, Time.fixedDeltaTime * 4f);

            rb.MoveRotation(finalRotation);
        }

        //Acceleration
        Vector3 force = desiredDireection * acceleration;
        rb.AddForce(force, ForceMode.Acceleration);

        // Speed limiting
        float currentMaxSpeed = (brakingZoneSpeed > 0) ? brakingZoneSpeed : maxSpeed;

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * currentMaxSpeed;
        }

        // switches Waypoints
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= waypointRange)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Count)
            {
                currentWaypoint = 0;
            }
        }

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

    // Braking Zones entering and exiting
    private void OnTriggerEnter(Collider other)
    {
        BrakingZone zone = other.GetComponent<BrakingZone>();
        if (zone != null)
        {
            brakingZoneSpeed = zone.targetSpeed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BrakingZone zone = other.GetComponent<BrakingZone>();
        if (zone != null)
        {
            brakingZoneSpeed = -1f;
        }
    }
}