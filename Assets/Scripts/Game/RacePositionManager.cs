using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RacePositionManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text playerPositionText;      // e.g. "1 / 4"
    public TMP_Text[] leaderboardLines;      // optional list lines: 1. Name

    private List<RacerInfo> racers = new List<RacerInfo>();
    private int totalRacers;

    void Start()
    {
        racers.AddRange(FindObjectsOfType<RacerInfo>());
        totalRacers = racers.Count;

        if (totalRacers == 0)
            Debug.LogWarning("RacePositionManager: No RacerInfo components found!");
    }

    void Update()
    {
        if (racers.Count == 0) return;

        // Sort racers by progress (highest first)
        racers.Sort((a, b) => GetProgress(b).CompareTo(GetProgress(a)));

        // Update player position UI
        int playerIndex = racers.FindIndex(r => r.isPlayer);
        if (playerIndex >= 0 && playerPositionText != null)
        {
            int position = playerIndex + 1;
            playerPositionText.text = position + " / " + totalRacers;
        }

        // Optional leaderboard lines
        if (leaderboardLines != null && leaderboardLines.Length > 0)
        {
            for (int i = 0; i < leaderboardLines.Length; i++)
            {
                if (i < racers.Count)
                {
                    var r = racers[i];
                    string status = r.hasFinished ? "FIN" : $"Lap {Mathf.Clamp(r.currentLap + 1, 1, r.totalLaps)}";
                    leaderboardLines[i].text = $"{i + 1}. {r.racerName}  ({status})";
                }
                else
                {
                    leaderboardLines[i].text = "";
                }
            }
        }
    }

    float GetProgress(RacerInfo r)
    {
        // Larger = further ahead
        // currentLap is 0-based, so add 1.0 to avoid all zeros at start
        float lapPart = r.currentLap;
        int waypointCount = (r.waypointContainer != null && r.waypointContainer.waypoints != null)
                            ? r.waypointContainer.waypoints.Count
                            : 0;

        float waypointPart = (waypointCount > 0) ? (float)r.currentWaypoint / waypointCount : 0f;

        // Distance is subtracted (closer to next waypoint = slightly higher progress)
        float distanceTieBreaker = -r.distanceToNext * 0.001f;

        return lapPart + waypointPart + distanceTieBreaker;
    }
}