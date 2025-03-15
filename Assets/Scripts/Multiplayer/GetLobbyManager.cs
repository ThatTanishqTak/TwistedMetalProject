using UnityEngine;
using UnityEngine.UI;

public class GetLobbyManager : MonoBehaviour
{
    private LobbySystem lobbySystem;
    private Button button;

    private void Start()
    {
        lobbySystem = FindFirstObjectByType<LobbySystem>();
        button = FindFirstObjectByType<Button>();

        button.onClick.AddListener(lobbySystem.OnJoinLobbyButtonClicked);
    }
}