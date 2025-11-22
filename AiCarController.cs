using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PhysicsController))]
public class AiCarController : MonoBehaviour
{
    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;
    public int currentWaypoint;
    private PhysicsController carController;
    public float waypointRange;
    private float currecntAngle;
    private float gasInput;
    public float gasDampen;
    public bool isInsideBraking;
    public float maximumAngle = 45f;
    public float maximumSpeed = 120f;
    [Range(0.01f,0.04f)]
    public float turningConstant = 0.02f;
    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<PhysicsController>();
        waypoints = waypointContainer.waypoints;
        currentWaypoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(waypoints[currentWaypoint].position, transform.position) < waypointRange)
        {
            currentWaypoint++;
            if (currentWaypoint == waypoints.Count) currentWaypoint = 0;
        }
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        currecntAngle = Vector3.SignedAngle(fwd, waypoints[currentWaypoint].position - transform.position, Vector3.up);
        gasInput = Mathf.Clamp01((1f - Mathf.Abs(carController.speed * 0.01f * currecntAngle) / (maximumAngle)));
        if (isInsideBraking)
        {
            gasInput = -gasInput((Mathf.Clamp01((carController.speed / maximumSpeed) * 2 - 1f)));
        }
        gasDampen = Mathf.Lerp(gasDampen, gasInput, Time.deltaTime * 3f);
        carController.setInput(gasDampen, currecntAngle, 0, 0); //tcontrolls ai kart from player kart
        Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.yellow);
    }

   
}
