using UnityEngine;
using TMPro;

public class SpeedometerUI : MonoBehaviour
{
    public Rigidbody playerRB;
    public TMP_Text speedText;

    private const float ToMPH = 2.23694f;

    void Start()
    {
        // Auto-find the player if not assigned
        if (playerRB == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerRB = player.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (playerRB == null || speedText == null)
            return;

        float speedMPH = playerRB.velocity.magnitude * ToMPH;

        speedText.text = speedMPH.ToString("000");
    }
}