using UnityEngine;
using TMPro;


// Displays the player's kart speed in MPH.

public class SpeedometerUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Rigidbody of the player kart. Auto-assigned if left empty.")]
    public Rigidbody playerRB;

    [Tooltip("UI text field where speed will be displayed.")]
    public TMP_Text speedText;

    private const float ToMPH = 2.23694f;

    void Start()
    {
        AssignPlayerRigidbody();
    }

    void Update()
    {
        // Safeguard: reattempt auto-assignment if lost (scene reload, prefab spawn, etc.)
        if (playerRB == null)
            AssignPlayerRigidbody();

        if (playerRB == null || speedText == null)
            return;

        UpdateSpeedometer();
    }


    /// Attempts to automatically find and cache the player's Rigidbody.
    private void AssignPlayerRigidbody()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerRB = player.GetComponent<Rigidbody>();
    }


    /// Computes and updates the speed text display.
    private void UpdateSpeedometer()
    {
        float speedMPH = playerRB.velocity.magnitude * ToMPH;
        speedText.text = speedMPH.ToString("000");
    }
}

