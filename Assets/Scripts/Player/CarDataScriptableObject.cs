using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

[CreateAssetMenu(fileName = "CarDataScriptableObject", menuName = "Scriptable Objects/CarDataScriptableObject")]
public class CarDataScriptableObject : ScriptableObject
{
    public float Health = 100;
    public float Speed = 100;
}