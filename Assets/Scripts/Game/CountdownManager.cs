using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public TMP_Text countdownText;
    public RaceTimer raceTimer;

    private PlayerKartController playerKart;  // we keep this, but we fill it automatically

    IEnumerator Start()
    {
        // Wait 1 frame so KartLoader can spawn the kart first
        yield return null;

        // Automatically find the spawned kart
        playerKart = Object.FindFirstObjectByType<PlayerKartController>();

        if (playerKart == null)
        {
            Debug.LogWarning("CountdownManager: No PlayerKartController found in scene!");
        }

        StartCoroutine(CountdownSequence());
    }

    IEnumerator CountdownSequence()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";
        raceTimer.StartRace();

        if (playerKart != null)
        {
            playerKart.canDrive = true;
            Debug.Log("Player can now drive!");
        }

        yield return new WaitForSeconds(1f);
        countdownText.text = "";
    }
}
