using Unity.Netcode;
using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;

    private void Start()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerManager_OnPlayerDataNetworkListChanged;
        CharacterReady.Instance.OnReadyChanged += CharacterSelect_OnReadyChanged;

        UpdatePlayer();
    }

    private void CharacterSelect_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void MultiplayerManager_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (MultiplayerManager.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
            
            PlayerData playerData = MultiplayerManager.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyGameObject.SetActive(CharacterReady.Instance.IsPlayerReady(playerData.clientID));
        }

        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= MultiplayerManager_OnPlayerDataNetworkListChanged;
    }
}