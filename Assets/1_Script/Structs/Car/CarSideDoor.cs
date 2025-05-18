using Garage.Controller;
using UnityEngine;

using Garage.Structs.CarPart;
public class CarSideDoor : MonoBehaviour
{
    private CarController car;
    public CarController Car => car;

    private void Awake()
    {
        car = GetComponentInParent<CarController>();
        if (car == null)
            Debug.LogWarning("Can't detect parent's CarController component");
    }
}
