using Cinemachine;
using UnityEngine;

public class POVCamera : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_startCamera;
    [SerializeField] private CinemachineVirtualCamera m_followCamera;
    [SerializeField] private float m_waitBeforeSwitch = 2;
    private void Start()
    {
        Invoke(nameof(SwitchCamera), m_waitBeforeSwitch);
    }

    private void SwitchCamera()
    {
        m_startCamera.Priority = 0;
        m_followCamera.Priority = 10;
    }
}
