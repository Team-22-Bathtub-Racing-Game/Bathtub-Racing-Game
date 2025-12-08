using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class RacePositionManager : MonoBehaviour
{
    public TMP_Text positionText;

    private List<RacerInfo> racers = new List<RacerInfo>();

    void Start()
    {
        // FIND ALL RACERS IN THE SCENE
        racers = FindObjectsOfType<RacerInfo>().ToList();

        if (racers.Count == 0)
            Debug.LogError("NO RACERS FOUND! Add RacerInfo to all karts.");
    }
    public void RefreshRacers()
    {
        racers = FindObjectsOfType<RacerInfo>().ToList();
        Debug.Log("Racers refreshed! Count = " + racers.Count);
    }

    void Update()
    {
        SortRacers();
        UpdatePositionDisplay();
    }

    void SortRacers()
    {
        racers = racers.OrderByDescending(r => r.currentLap)
                       .ThenByDescending(r => r.currentWaypoint)
                       .ThenBy(r => r.distanceToNext)
                       .ToList();
    }

    void UpdatePositionDisplay()
    {
        RacerInfo player = racers.FirstOrDefault(r => r.isPlayer);

        if (player == null)
        {
            Debug.LogError("PLAYER NOT FOUND IN RACER LIST!");
            return;
        }

        int position = racers.IndexOf(player) + 1;

        positionText.text = "Place: " + position + " / " + racers.Count;
    }
}
