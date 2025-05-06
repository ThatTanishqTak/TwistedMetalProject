using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SandstormManager : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("sandstorm VFX")]
    [SerializeField] private GameObject sandstormEffect;
    [Tooltip("UI panel that warns Sandstorm incoming")]
    [SerializeField] private GameObject warningPanel;

    [Header("Timing Settings")]
    [Tooltip("Random delay before each storm")]
    [SerializeField] private float minDelay = 20f, maxDelay = 40f;
    [Tooltip("How long the warning panel shows before storm")]
    [SerializeField] private float warningDuration = 5f;
    [Tooltip("How long the sandstorm lasts")]
    [SerializeField] private float stormDuration = 30f;

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(StormCycle());
        }
        else
        {
            if (sandstormEffect) sandstormEffect.SetActive(false);
            if (warningPanel) warningPanel.SetActive(false);
        }
    }

    private IEnumerator StormCycle()
    {
        while (true)
        {
            float wait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(wait);

            WarningClientRpc(true);
            yield return new WaitForSeconds(warningDuration);
            WarningClientRpc(false);

            StormClientRpc(true);
            yield return new WaitForSeconds(stormDuration);

            StormClientRpc(false);
        }
    }

    [ClientRpc]
    private void WarningClientRpc(bool show)
    {
        if (warningPanel) warningPanel.SetActive(show);
    }

    [ClientRpc]
    private void StormClientRpc(bool start)
    {
        if (sandstormEffect) sandstormEffect.SetActive(start);
        // broadcast to cars
        if (start) StormController.StormBeganClientStatic();
        else StormController.StormEndedClientStatic();
    }
}
