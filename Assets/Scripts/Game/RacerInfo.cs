using UnityEngine;

public class RacerInfo : MonoBehaviour
{
    [Header("Identity")]
    public string racerName = "Racer";
    public bool isPlayer = false;

    [Header("Race Progress")]
    public int currentLap = 0;         // 0-based
    public int totalLaps = 3;
    public int currentWaypoint = 0;    // waypoint index
    public float distanceToNext = 0f;
    public bool hasFinished = false;

    public WaypointContainer waypointContainer { get; private set; }

    OpponentKartAI ai;
    Transform tr;

    void Awake()
    {
        tr = transform;
        ai = GetComponent<OpponentKartAI>();

            waypointContainer = FindFirstObjectByType<WaypointContainer>();  

        // Get total laps from Track Selection
        totalLaps = PlayerPrefs.GetInt("SelectedLapCount", totalLaps);

        // Use AI's waypointContainer if available
        if (ai != null)
            waypointContainer = ai.waypointContainer;

        if (waypointContainer == null)
            Debug.LogError("RacerInfo: No WaypointContainer found!");
    }

    void Update()
    {
        if (waypointContainer == null ||
            waypointContainer.waypoints == null ||
            waypointContainer.waypoints.Count == 0)
            return;

        var list = waypointContainer.waypoints;

        if (ai != null)
        {
            currentWaypoint = Mathf.Clamp(ai.currentWaypoint, 0, list.Count - 1);
            distanceToNext = Vector3.Distance(tr.position, list[currentWaypoint].position);
        }
        else
        {
            int bestIndex = 0;
            float best = float.MaxValue;

            for (int i = 0; i < list.Count; i++)
            {
                float d = (tr.position - list[i].position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestIndex = i;
                }
            }

            currentWaypoint = bestIndex;
            distanceToNext = Mathf.Sqrt(best);
        }
    }
}
