using UnityEngine;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    public TMP_Text timerText;
    public bool raceActive = false;
    private float raceTime = 0f;

    void Update()
    {
        if (raceActive)
        {
            raceTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(raceTime / 60);
        int seconds = Mathf.FloorToInt(raceTime % 60);
        int milliseconds = Mathf.FloorToInt((raceTime * 1000) % 1000);

        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }

    public void StartRace() => raceActive = true;
    public void StopRace() => raceActive = false;
}
