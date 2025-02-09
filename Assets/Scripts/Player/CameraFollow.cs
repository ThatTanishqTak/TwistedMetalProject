using UnityEngine;
using Unity.Cinemachine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField]private bool isFollowing = false;
    void Start()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        Invoke("FindAndAssignPlayer", 0.7f);
    }

    private void FindAndAssignPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            cinemachineCamera.Follow = player.transform;
            isFollowing = true;
            Debug.Log($"Following the player: {isFollowing}");
        }
        else
        {
            Debug.Log($"Not following the player: {isFollowing}");
            Invoke("FindAndAssignPlayer", 0.5f);
        }
    }
   
}
