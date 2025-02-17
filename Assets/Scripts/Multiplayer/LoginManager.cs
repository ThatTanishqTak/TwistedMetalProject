using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class LoginManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField codeInputField;

    public NetworkVariable<string> username = new();
}