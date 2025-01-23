using System;
using Dreamteck.Splines;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIControl : MonoBehaviour
{
    private const float MAX_WAIT_TIME = 10f;
    private const float MAX_REVERSE_TIME = 10f;
    [SerializeField] private float m_checkDistance = 0.5f;
    [SerializeField] private SplineComputer m_splineComputer;
    [SerializeField] private bool m_loopPath;
    [SerializeField] private Color m_bodyColor;
    [SerializeField] private MeshRenderer m_bodyMeshRenderer;
    
    [Header("Sensors Information")]
    [SerializeField] private Vector3 m_sensorCenterOffset;
    [SerializeField] private float m_sensorLength = 10f;
    [SerializeField] private float m_sideSensorAngle = 25f;
    [SerializeField] private float m_sideSensorDistance = 1f;
    [SerializeField] private LayerMask m_targetLayer;
    
    private VehicleController m_vehicleController;
    private SplinePoint[] m_splinePoints;
    private int m_currentPoint;
    private Vector3 m_targetPosition;
    private bool m_avoidingObstacle;
    private float m_waitTime;
    private float m_reverseTime;
    private bool m_isTakingReverse;
    private void Start()
    {
        m_vehicleController = GetComponent<VehicleController>();
        SetTracksPoints(m_splineComputer.GetPoints());
    }
    
    
    private void SetTracksPoints(SplinePoint[] trackPoints)
    {
        m_splinePoints = trackPoints;
        m_currentPoint = 1;
        m_targetPosition = m_splinePoints[m_currentPoint].position;
        m_vehicleController.Handbrake = false;
    }
    
    private void FixedUpdate()
    {
        if (m_isTakingReverse)
        {
            if (m_reverseTime > MAX_REVERSE_TIME)
            {
                m_isTakingReverse = false;
            }
            else
            {
                m_reverseTime += Time.fixedDeltaTime;
            }
            m_vehicleController.Throttle = -0.5f;
            return;
        }
        Sensors();
        if (!m_avoidingObstacle)
        {
            Move();
        }
        CheckStuck();
    }

    private void CheckStuck()
    {
        if (m_avoidingObstacle && m_vehicleController.GetVelocity() <= 0.025f)
        {
            if (m_waitTime < MAX_WAIT_TIME)
            {
                m_waitTime += Time.fixedDeltaTime;
            }
            else
            {
                int spawnPoint = m_currentPoint - 1;
                if (spawnPoint < 0)
                {
                    spawnPoint = 0;
                }
                //m_vehicleController.ResetPosition(m_splinePoints[spawnPoint].position, Quaternion.LookRotation(m_splinePoints[spawnPoint + 1].position - m_splinePoints[spawnPoint].position));
                m_waitTime = 0;
                m_reverseTime = 0;
                m_isTakingReverse = true;
            }
        }
        else
        {
            m_waitTime = 0;
        }
    }
    
    private void Move()
    {
        if (CheckDistance(transform.position, m_targetPosition) > m_checkDistance)
        {
            Vector3 relativeVector = transform.InverseTransformPoint(m_targetPosition);
            float steerAngle = (relativeVector.x / relativeVector.magnitude) * m_vehicleController.SteerAngle;
            m_vehicleController.SetSteeringAngle(steerAngle);
            m_vehicleController.Throttle = 1f;
        }
        else
        {
            m_currentPoint++;
            if (m_currentPoint < m_splinePoints.Length)
            {
                m_targetPosition = m_splinePoints[m_currentPoint].position;
            }
            else
            {
                if (m_loopPath)
                {
                    m_currentPoint = 0;
                    m_targetPosition = m_splinePoints[m_currentPoint].position;
                }
                else
                {
                    m_vehicleController.Throttle = 0f;
                    //m_vehicleController.Handbrake = true;
                }
            }
        }
    }
    
    private void Sensors()
    {
        m_avoidingObstacle = false;
        float m_steerMultipler = 0;
        RaycastHit hit;
        Vector3 sensorCenterPosition = transform.position;
        sensorCenterPosition += transform.forward * m_sensorCenterOffset.z;
        sensorCenterPosition += transform.up * m_sensorCenterOffset.y;
        //Right
        Vector3 sensorSidePosition = transform.right * m_sideSensorDistance + sensorCenterPosition;
        if (CastRay(sensorSidePosition, transform.forward, out hit))
        {
            m_avoidingObstacle = true;
            m_steerMultipler -= 1f;
        }
        //Right Angle
        if (CastRay(sensorSidePosition, Quaternion.AngleAxis(m_sideSensorAngle, transform.up) * transform.forward, out hit))
        {
            m_avoidingObstacle = true;
            m_steerMultipler -= 0.5f;
        }
        //Left
        sensorSidePosition -= transform.right * (m_sideSensorDistance * 2);
        if (CastRay(sensorSidePosition, transform.forward, out hit))
        {
            m_avoidingObstacle = true;
            m_steerMultipler += 1f;
        }
        //Left Angle
        if (CastRay(sensorSidePosition, Quaternion.AngleAxis(-m_sideSensorAngle, transform.up) * transform.forward, out hit))
        {
            m_avoidingObstacle = true;
            m_steerMultipler += 0.5f;
        }
        //Center
        if (m_steerMultipler == 0 && CastRay(sensorCenterPosition, transform.forward, out hit))
        {
            m_avoidingObstacle = true;
            //m_steerMultipler = (hit.normal.x < 0) ? -1f : 1f;
            m_vehicleController.Throttle = 0;
            return;
        }
        if (m_avoidingObstacle)
        {
            m_vehicleController.Steering = m_steerMultipler;
            m_vehicleController.Throttle = 0.3f;
        }
    }

    private bool CastRay(Vector3 origin, Vector3 direction, out RaycastHit hit)
    {
        if (Physics.Raycast(origin, direction, out hit, m_sensorLength, m_targetLayer))
        {
            Debug.DrawLine(origin, hit.point, Color.red);
            return true;
        }
        return false;
    }

    private float CheckDistance(Vector3 startPosition, Vector3 endposition)
    {
        Vector3 vectorToTarget = startPosition - endposition;
        vectorToTarget.y = 0;
        return vectorToTarget.magnitude;
    }

    private void OnValidate()
    {
        if (m_bodyMeshRenderer != null)
        {
            m_bodyMeshRenderer.sharedMaterial.color = m_bodyColor;
        }
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
            return;
        Debug.DrawLine(transform.position, m_targetPosition, Color.green);
    }
}
