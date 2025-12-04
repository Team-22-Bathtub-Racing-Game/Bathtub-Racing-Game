using System.Collections;
using TMPro;
using UnityEngine;

public class LapCounter : MonoBehaviour
{
    public int totalLaps = 3;
    public int currentLap = 1;   // 1-based for UI

    public TMP_Text lapText;
    private bool canTriggerLap = true;
    private bool raceStarted = false;

    [Header("Player Racer Reference")]
    public RacerInfo playerRacer;   // assign in inspector OR auto-find

    void Start()
    {
        totalLaps = PlayerPrefs.GetInt("SelectedLapCount", 3);

        currentLap = 1;
        raceStarted = false;

        if (playerRacer == null)
        {
            // Find the player’s RacerInfo automatically
            RacerInfo[] infos = Object.FindObjectsOfType<RacerInfo>();
            foreach (var r in infos)
                if (r.isPlayer)
                    playerRacer = r;
        }

        if (playerRacer != null)
        {
            playerRacer.currentLap = 0;      // 0-based internal
            playerRacer.totalLaps = totalLaps;
        }

        UpdateLapDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        // PLAYER
        if (other.CompareTag("Player") && canTriggerLap)
        {
            AdvanceLap();
        }
        // AI – update their RacerInfo only (no UI changes)
        else if (other.CompareTag("AI"))
        {
            var aiRacer = other.GetComponent<RacerInfo>();
            if (aiRacer != null && !aiRacer.hasFinished)
            {
                aiRacer.currentLap++;

                if (aiRacer.currentLap >= aiRacer.totalLaps)
                    aiRacer.hasFinished = true;
            }
        }
    }

    void AdvanceLap()
    {
        if (!raceStarted)
        {
            raceStarted = true;
            Debug.Log("Race officially started!");
            StartCoroutine(LapCooldown());
            return;
        }
        currentLap++;

        if (playerRacer != null)
        {
            playerRacer.currentLap = currentLap - 1;

            if (currentLap > totalLaps)
                playerRacer.hasFinished = true;
        }

        // Check if race finished
        if (currentLap > totalLaps)
        {
            lapText.text = "FINISHED!";
            Object.FindFirstObjectByType<RaceTimer>().StopRace();
            return;
        }

        UpdateLapDisplay();
        StartCoroutine(LapCooldown());
    }

    IEnumerator LapCooldown()
    {
        canTriggerLap = false;
        yield return new WaitForSeconds(1f);
        canTriggerLap = true;
    }

    void UpdateLapDisplay()
    {
        lapText.text = "Lap " + currentLap + " / " + totalLaps;
    }
}