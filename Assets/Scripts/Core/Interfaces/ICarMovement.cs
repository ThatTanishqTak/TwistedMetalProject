using UnityEngine;

public interface ICarMovement
{
    void Move(float throttle, float steering);
    void ApplyHandbrake(bool active);
}
