using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        // Check to make sure we are spawning the right character
        if (Runner.LocalPlayer == player) { Runner.Spawn(PlayerPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity); }
    }
}
