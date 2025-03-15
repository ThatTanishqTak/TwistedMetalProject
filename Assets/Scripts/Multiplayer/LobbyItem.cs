using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    public event System.Action<Lobby> OnLobbySelected;

    public void Setup(Lobby lobby, System.Action<Lobby> onSelected)
    {
        OnLobbySelected = onSelected;

        var nameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        var playerText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        nameText.text = lobby.Name;
        playerText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void SelectLobby()
    {
        OnLobbySelected?.Invoke(null); // TODO(Tanishq): Pass actual lobby data
    }
}