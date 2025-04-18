using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private bool isShooterControlled;

    private void Update()
    {
        if (!isShooterControlled) return;

        if (Input.GetMouseButton(0))
        {
            Debug.Log("Mouse Clicked");
            gun.Fire();
        }
    }

    public void SetShooterAuthority(bool value)
    {
        isShooterControlled = value;
    }
}
