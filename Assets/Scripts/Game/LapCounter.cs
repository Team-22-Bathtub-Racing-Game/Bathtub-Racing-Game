using System.Collections;
using TMPro;
using UnityEngine;

public class LapCounter : MonoBehaviour
{
    public int totalLaps = 3;
    public int currentLap = 1;

    public TMP_Text lapText;
    private bool canTriggerLap = true;
    private bool raceStarted = false;

    void Start()
    {
        UpdateLapDisplay(); // Show Lap 1 / totalLaps at start
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canTriggerLap)
        {
            AdvanceLap();
        }
    }

    void AdvanceLap()
    {
        if (!raceStarted)
        {
            // First crossing just starts the race
            raceStarted = true;
            Debug.Log("Race officially started!");
            StartCoroutine(LapCooldown());
            return;
        }

        // Increment lap for subsequent crossings
        currentLap++;

        // Update display
        UpdateLapDisplay();

        // Check if race finished
        if (currentLap > totalLaps)
        {
            lapText.text = "FINISHED!";
            Object.FindFirstObjectByType<RaceTimer>().StopRace();
            return;
        }

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



