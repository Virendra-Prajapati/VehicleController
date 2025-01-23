using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelMeshController : MonoBehaviour
{
    [Header("Wheel Settings")]
    [SerializeField] private GameObject wheelMesh;           // The 3D model of the wheel
    [SerializeField] private bool mirrorWheel;               // Whether to mirror the wheel's rotation (for certain vehicle types)
    [SerializeField] private Vector3 mirrorOffset = new Vector3(0, 180, 0); // The offset to apply when mirroring the wheel

    private WheelCollider wheelCollider;

    private void Awake()
    {
        // Get the WheelCollider component attached to this GameObject
        wheelCollider = GetComponent<WheelCollider>();
    }

    private void FixedUpdate()
    {
        UpdateWheelPositionAndRotation();
    }

    private void UpdateWheelPositionAndRotation()
    {
        // Get the position and rotation of the wheel collider
        wheelCollider.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);

        // Apply the position and rotation to the wheel mesh
        wheelMesh.transform.position = wheelPosition;
        wheelMesh.transform.rotation = wheelRotation;

        // If mirroring is enabled, apply the mirror offset
        if (mirrorWheel)
        {
            wheelMesh.transform.localRotation *= Quaternion.Euler(mirrorOffset);
        }
    }
}