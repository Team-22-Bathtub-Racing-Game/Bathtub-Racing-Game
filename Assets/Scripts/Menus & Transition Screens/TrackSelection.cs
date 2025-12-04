using UnityEngine;
using TMPro;

public class TrackSelectionManager : MonoBehaviour
{
    public TMP_InputField lapsInput; // For 'Number of Laps'
    public TMP_InputField opponentsInput; // For 'Number of Opponents'

    public void ConfirmAndStartRace()
    {
        int laps = 3;

        if (!int.TryParse(lapsInput.text, out laps))
            laps = 3;

        // Lap Limit
        laps = Mathf.Clamp(laps, 1, 50);

        // Save it to LapCounter
        PlayerPrefs.SetInt("SelectedLapCount", laps);
        PlayerPrefs.Save();

        Debug.Log("TrackSelection: Saved Laps = " + laps);

        // Load the race scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Racetrack");
    }
}
