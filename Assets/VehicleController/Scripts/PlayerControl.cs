using System;
using UnityEngine;
[RequireComponent(typeof(VehicleController))]
public class PlayerControl : MonoBehaviour
{
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string VERTICAL_AXIS = "Vertical";
        
    private VehicleController m_vehicleController;

    public Mode mode;
    private void Start()
    {
        m_vehicleController = GetComponent<VehicleController>();
    }

    private void Update()
    {
        m_vehicleController.Throttle = Input.GetAxis(VERTICAL_AXIS);
        m_vehicleController.Steering = Input.GetAxis(HORIZONTAL_AXIS);
        m_vehicleController.Boosting = Input.GetKey(KeyCode.LeftShift);
        switch (mode)
        {
            case Mode.HANDBRAKE:
                m_vehicleController.Handbrake = Input.GetKey(KeyCode.Space);
                break;
            case Mode.JUMP:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_vehicleController.Jump();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
        
    public enum Mode
    {
        HANDBRAKE,
        JUMP
    }
}
