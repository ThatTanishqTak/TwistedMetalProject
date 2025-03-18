using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float duration = 5f; // ✅ Power-up lasts for 5 seconds

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Power-up triggered by: " + other.name); // ✅ Debugging trigger detection

        if (other.CompareTag("Player")) // Ensure only the player picks it up
        {
            Debug.Log("Player picked up power-up!"); // ✅ Confirm pickup

            // ✅ Find the ShootPoint that has the "ShootPoint" tag
            GameObject shootPointObject = GameObject.FindGameObjectWithTag("ShootPoint");
            if (shootPointObject != null)
            {
                Shooting shooting = shootPointObject.GetComponent<Shooting>(); // ✅ Get the Shooting script from ShootPoint

                if (shooting != null)
                {
                    Debug.Log("Shooting script found! Applying power-up."); // ✅ Confirm script found
                    shooting.DoubleFireRate(duration); // ✅ Call the fire rate boost
                }
                else
                {
                    Debug.LogError("Shooting script not found on ShootPoint!");
                }
            }
            else
            {
                Debug.LogError("ShootPoint with 'ShootPoint' tag not found!");
            }

            gameObject.SetActive(false); // ✅ Hide the power-up
        }
    }


}
