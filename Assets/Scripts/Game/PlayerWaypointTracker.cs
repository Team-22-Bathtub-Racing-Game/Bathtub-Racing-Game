using UnityEngine;

[RequireComponent(typeof(RacerInfo))]
public class PlayerWaypointTracker : MonoBehaviour
{
    private RacerInfo racerInfo;
    private WaypointContainer waypointContainer;

    void Start()
    {
        racerInfo = GetComponent<RacerInfo>();
        waypointContainer = FindFirstObjectByType<WaypointContainer>();

        racerInfo.totalLaps = PlayerPrefs.GetInt("SelectedLapCount", 3);
    }

    void FixedUpdate()
    {
        if (waypointContainer == null || racerInfo == null)
            return;

        var waypoints = waypointContainer.waypoints;

        float minDist = float.MaxValue;
        int closest = 0;

        // Find closest waypoint
        for (int i = 0; i < waypoints.Count; i++)
        {
            float d = Vector3.Distance(transform.position, waypoints[i].position);
            if (d < minDist)
            {
                minDist = d;
                closest = i;
            }
        }

        racerInfo.currentWaypoint = closest;
        racerInfo.distanceToNext = minDist;
    }
}