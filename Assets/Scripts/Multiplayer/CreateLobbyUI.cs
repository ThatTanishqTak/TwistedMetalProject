using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobby;

    [SerializeField] private TMP_InputField lobbyName;
    [SerializeField] private TMP_InputField maxPlayers;

    private void Awake()
    {
        createLobby.onClick.AddListener(() =>
        {
            if (int.TryParse(maxPlayers.text, out int playerCount))
            {
                GameLobby.Instance.CreateLobby(lobbyName.text, playerCount, true);
            }
            else
            {
                Debug.LogError("Invalid player count entered. Please enter a valid number.");
            }
        });
    }
}