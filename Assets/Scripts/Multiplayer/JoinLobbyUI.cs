using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyUI : MonoBehaviour
{
    [SerializeField] private Button joinButton;

    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_InputField lobbyCode;

    private void Awake()
    {
        joinButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinLobbyByCode(lobbyCode.text);
        });
    }
}