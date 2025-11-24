using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public TMP_Text countdownText;
    public RaceTimer raceTimer;     // assign race timer
    public PlayerKartController playerKart;  // assign player's kart controller

    void Start()
    {
        playerKart.canDrive = false;     // freeze kart
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
        playerKart.canDrive = true;      // UNFREEZE kart!
        yield return new WaitForSeconds(1f);

        countdownText.text = "";         // hide countdown
    }
}
