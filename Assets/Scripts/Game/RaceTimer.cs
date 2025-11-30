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
        int milliseconds = Mathf.FloorToInt((raceTime * 100) % 100);

        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    public void StartRace() => raceActive = true;
    public void StopRace() => raceActive = false;
}
