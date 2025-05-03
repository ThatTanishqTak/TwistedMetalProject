using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Weapons/GunStats")]
public class GunStats : ScriptableObject
{
    [Header("GunStats")]
    public string weaponName;
    public float fireRate = 0.25f;
    public float damage;
    public float bulletRange;
    public int ammo = -1;
}
