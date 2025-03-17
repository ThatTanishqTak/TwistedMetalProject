using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button menuButton;
    [SerializeField] private Button readyButton;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Awake()
    {
        menuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            //SceneManager.LoadScene()
        });

        readyButton.onClick.AddListener(() =>
        {
            CharacterReady.Instance.SetPlayerReady();
        });
    }

    private void Start()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();
        
        lobbyNameText.text = lobby.Name;
        lobbyCodeText.text = lobby.LobbyCode;
    }
}