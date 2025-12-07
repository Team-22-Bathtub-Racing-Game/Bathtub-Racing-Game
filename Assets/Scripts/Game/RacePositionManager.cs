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
        RefreshRacers();
    }

    public void RefreshRacers()
    {
        racers = FindObjectsOfType<RacerInfo>()
            .Where(r => r != null)
            .Distinct()
            .ToList();

        Debug.Log("Racers refreshed! Count = " + racers.Count);
    }

    void Update()
    {
        if (racers.Count == 0) return;

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
            positionText.text = "ERR";
            return;
        }

        int position = racers.IndexOf(player) + 1;
        positionText.text = position + "/" + racers.Count;
    }

    public int GetPlayerPosition()
    {
        SortRacers();
        RacerInfo player = racers.First(r => r.isPlayer);
        return racers.IndexOf(player) + 1;
    }
}


