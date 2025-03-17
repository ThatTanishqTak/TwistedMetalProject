using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobby;

    [SerializeField] private TMP_InputField lobbyName;
    [SerializeField] private TMP_InputField maxPlayers;

    private void Awake()
    {
        createLobby.onClick.AddListener(() =>
        {
            GameLobby.Instance.CreateLobby(lobbyName.text, 4, true);
        });
    }
}