using UnityEngine;

public class CarAdapter : MonoBehaviour, ICarMovement
{
    [SerializeField] private PrometeoCarController prometeo;

    public void Move(float throttle, float steering)
    {
        if (prometeo == null) return;

        if (throttle > 0) prometeo.GoForward();
        else if (throttle < 0) prometeo.GoReverse();
        else prometeo.Brakes();

        prometeo.Steer(steering);
    }

    public void ApplyHandbrake(bool active) {
        if (active) {
            prometeo.Handbrake();
        }
        else{
            prometeo.RecoverTraction();
        }
    }
}
