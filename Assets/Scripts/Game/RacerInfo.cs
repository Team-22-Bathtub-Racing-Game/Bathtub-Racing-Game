using UnityEngine;

public class RacerInfo : MonoBehaviour
{
    [Header("Identity")]
    public string racerName = "Racer";
    public bool isPlayer = false;

    [Header("Race Progress (read-only at runtime)")]
    public int currentLap = 0;        // 0-based internally
    public int totalLaps = 3;
    public int currentWaypoint = 0;
    public float distanceToNext = 0f;
    public bool hasFinished = false;

    // Internal refs
    public WaypointContainer waypointContainer { get; private set; }
    private OpponentKartAI ai;
    private Transform tr;

    void Awake()
    {
        tr = transform;
        ai = GetComponent<OpponentKartAI>();

        // Get total laps from PlayerPrefs
        totalLaps = PlayerPrefs.GetInt("SelectedLapCount", totalLaps);

        // For AI: reuse its existing waypointContainer
        if (ai != null && ai.waypointContainer != null)
        {
            waypointContainer = ai.waypointContainer;
        }
        else
        {
            // For player (or fallback): auto-find the global WaypointContainer
            waypointContainer = FindObjectOfType<WaypointContainer>();

            if (waypointContainer == null)
            {
                Debug.LogWarning($"RacerInfo on {name}: No WaypointContainer found in scene.");
            }
        }
    }

    void Update()
    {
        if (waypointContainer == null ||
            waypointContainer.waypoints == null ||
            waypointContainer.waypoints.Count == 0)
            return;

        var list = waypointContainer.waypoints;

        // --- AI: use OpponentKartAI's currentWaypoint ---
        if (ai != null)
        {
            currentWaypoint = Mathf.Clamp(ai.currentWaypoint, 0, list.Count - 1);
            distanceToNext = Vector3.Distance(tr.position, list[currentWaypoint].position);
        }
        else
        {
            // --- Player: approximate by nearest waypoint ---
            int bestIndex = 0;
            float bestSqr = float.MaxValue;
            Vector3 pos = tr.position;

            for (int i = 0; i < list.Count; i++)
            {
                float d = (list[i].position - pos).sqrMagnitude;
                if (d < bestSqr)
                {
                    bestSqr = d;
                    bestIndex = i;
                }
            }

            currentWaypoint = bestIndex;
            distanceToNext = Mathf.Sqrt(bestSqr);
        }
    }
}
