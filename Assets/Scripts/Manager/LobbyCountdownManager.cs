using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;    
using TMPro;     

public class LobbyCountdownManager : NetworkBehaviour
{
    [Header("Countdown Settings")]
    [Tooltip("Seconds before everyone is sent to the Arena")]
    [SerializeField] private int startSeconds = 30;

    [Header("UI")]
    [Tooltip("Drag in a UI Text or TextMeshProUGUI to show the timer; leave blank to skip UI.")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Start()
    {
        // Only the host/server starts the countdown.
        if (!IsServer) return;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        int remaining = startSeconds;
        while (remaining > 0)
        {
            // update everyone’s UI
            UpdateTimerClientRpc(remaining);
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        // final “00” update
        UpdateTimerClientRpc(0);

        // teleport all clients into the Arena
        Loader.LoadNetwork(Loader.Scene.Arena);
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(int secondsLeft)
    {
        if (countdownText != null)
            countdownText.text = $"Match starts in {secondsLeft:00}";
    }
}
