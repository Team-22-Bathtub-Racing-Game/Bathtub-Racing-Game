using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LapCounter : MonoBehaviour
{
    public int totalLaps = 3;
    public int currentLap = 1;

    public TMP_Text lapText;       // Assign UI Text
    private bool canTriggerLap = true;

    void Start()
    {
        UpdateLapDisplay();
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
        currentLap++;

        if (currentLap > totalLaps)
        {
            // Race finished
            Object.FindFirstObjectByType<RaceTimer>().StopRace();
            lapText.text = "FINISHED!";
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



