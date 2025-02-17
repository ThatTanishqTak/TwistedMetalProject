using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;

    [SerializeField] private NetworkVariable<string> lobbyScene = new("Lobby");

    public override void OnNetworkSpawn()
    {
        NetworkManager.SetSingleton();
    }

    public async void OnHostButtonClicked(SceneEvent sceneEvent)
    {
        if (NetworkManager.Singleton != null)
        {
            if(sceneEvent.SceneName == "Lobby" && sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
            {
                if (IsHost)
                {

                }
            }
        }

        else
        {
            Debug.LogError("NetworkManager.Singleton is null!");
        }
    }

    private void OnServerButtonClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartServer();
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null!");
        }
    }

    private void OnClientButtonClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null!");
        }
    }
}