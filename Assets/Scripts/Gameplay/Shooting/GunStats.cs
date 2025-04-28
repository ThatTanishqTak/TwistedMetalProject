using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Weapons/GunStats")]
public class GunStats : ScriptableObject
{
    [Header("GunStats")]
    public string weaponName;

    //public GameObject projectilePrefab;
    public float fireRate = 0.25f;
    public float fireForce = 20f;

    public int ammo = -1;
}
