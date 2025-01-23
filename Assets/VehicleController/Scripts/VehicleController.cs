using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Wheels")]
    [SerializeField] private WheelCollider[] driveWheels;
    [SerializeField] private WheelCollider[] turnWheels;
    [SerializeField] private Transform centerOfMass;

    [Header("Behaviour")]
    [SerializeField] private AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);
    [SerializeField] private AnimationCurve motorTorqueCurve;
    [SerializeField] private float maxTorque = 500;
    [Range(30, 100)] [SerializeField] private float diffGearing = 30f;
    [SerializeField] private float brakeForce = 1500.0f;
    [Range(0, 1)] [SerializeField] private float decelerationMultiplier = 0.1f;
    [Range(0f, 50.0f)] [SerializeField] private float steerAngle = 30.0f;
    [Range(0.001f, 1.0f)] [SerializeField] private float steerSpeed = 0.2f;
    [Range(1f, 10f)] [SerializeField] private float jumpForce = 1.3f;
    [Range(1f, 10)] [SerializeField] private float driftMultiplier = 5f;
    [SerializeField] private bool handbrake;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float maxSpeedReverse = 10f;
    [SerializeField] private float speed;
    [SerializeField] private float boostForce = 5000;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem[] boostParticles;
    [SerializeField] private ParticleSystem[] driftParticles;
    [SerializeField] private TrailRenderer[] skidTrails;
    
    public bool IsGrounded => wheels.All(wheel => wheel.isGrounded);
    public bool Boosting { get => isBoosting; set => isBoosting = value; }
    public float Speed => speed;
    public bool Handbrake { get => handbrake; set => handbrake = value; }
    public float Throttle { get => throttle; set => throttle = Mathf.Clamp(value, -1f, 1f); }
    public float Steering { get => steering; set => steering = turnInputCurve.Evaluate(value) * steerAngle; }
    public float SteerAngle { get => steerAngle; set => steerAngle = Mathf.Clamp(value, 0.0f, 50.0f); }

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Rigidbody rb;
    private WheelCollider[] wheels;
    private WheelData[] wheelDatas;
    private float localVelocityX;
    private float localVelocityZ;
    private float driftingAxis;
    private bool isDrifting;
    private bool isTractionLocked;
    private bool isDecelerate;
    private bool isBoosting;
    private float steering;
    private float throttle;
    
    [System.Serializable]
    public struct WheelData
    {
        public WheelFrictionCurve FrictionCurve;
        public readonly float ExtremumSlip;
        public WheelData(WheelCollider wheelCollider)
        {
            FrictionCurve = new WheelFrictionCurve
            {
                extremumSlip = wheelCollider.sidewaysFriction.extremumSlip,
                extremumValue = wheelCollider.sidewaysFriction.extremumValue,
                asymptoteSlip = wheelCollider.sidewaysFriction.asymptoteSlip,
                asymptoteValue = wheelCollider.sidewaysFriction.asymptoteValue,
                stiffness = wheelCollider.sidewaysFriction.stiffness
            };
            ExtremumSlip = wheelCollider.sidewaysFriction.extremumSlip;
        }
    }
    
    private void Start()
    {        
        motorTorqueCurve = new AnimationCurve(new Keyframe(0, (maxTorque * 65) / 100), new Keyframe((maxSpeed * 25) / 100, maxTorque), new Keyframe(maxSpeed, 0));
        rb = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        if (centerOfMass == null) centerOfMass = transform;
        rb.centerOfMass = centerOfMass.localPosition;
        wheels = GetComponentsInChildren<WheelCollider>();
        wheelDatas = new WheelData[wheels.Length];
        for (int i = 0; i < wheels.Length; i++)
        {
            wheelDatas[i] = new WheelData(wheels[i]);
        }
    }

    private void FixedUpdate()
    {
        speed = (2 * Mathf.PI * turnWheels[0].radius * turnWheels[0].rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(rb.velocity).x;
        localVelocityZ = transform.InverseTransformDirection(rb.velocity).z;
        HandleSteering();
        HandleThrottle();
        HandleDecelerateCar();
        CheckHandBrake();
        RecoverTraction();
        HandleBoost();
        HandlePFX();
    }

    private void HandleSteering()
    {
        foreach (WheelCollider wheel in turnWheels)
        {
            wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed);
        }
    }
    
    private void HandleThrottle()
    {
        isDecelerate = false;
        if (throttle > 0)
        {
            //Forward
            if (localVelocityZ < -1f)
            {
                Brakes();
            }
            else
            {
                if (Mathf.RoundToInt(speed) < maxSpeed)
                {
                    ApplyThrottle();
                }
                else
                {
                    ThrottleOff();
                }
            }
        }
        else if(throttle < 0)
        {
            //Reverse
            if (localVelocityZ > 1f)
            {
                Brakes();
            }
            else
            {
                if (Mathf.Abs(Mathf.RoundToInt(speed)) < maxSpeedReverse)
                {
                    ApplyThrottle();
                }
                else
                {
                    ThrottleOff();
                }
            }
        }
        else
        {
            ThrottleOff();
            if (GetVelocity() > 0.025f)
            {
                isDecelerate = true;    
            }
        }
    }

    private void HandleDecelerateCar()
    {
        if (isDecelerate)
        {
            throttle = 0;
            rb.velocity = rb.velocity * 1f / (1f + 0.025f * decelerationMultiplier);
            ThrottleOff();
            if (GetVelocity() < 0.25f)
            {
                rb.velocity = Vector3.zero;
                isDecelerate = false;
            }      
        }
    }
    public float GetVelocity()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = 0;
        return velocity.magnitude;
    }

    private void ThrottleOff()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 0f;  
        }
    }
    
    private void RecoverTraction()
    {
        if (!isTractionLocked)
            return;
        isTractionLocked = false;
        driftingAxis -= (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }
        if (wheels[0].sidewaysFriction.extremumSlip > wheelDatas[0].ExtremumSlip)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                WheelData wheelData = wheelDatas[i];
                wheelData.FrictionCurve.extremumSlip = wheelData.ExtremumSlip * driftMultiplier * driftingAxis;
                wheelDatas[i] = wheelData;
                wheels[i].sidewaysFriction = wheelData.FrictionCurve;
            }
            isTractionLocked = true;
        }
        else if (wheels[0].sidewaysFriction.extremumSlip < wheelDatas[0].ExtremumSlip)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                WheelData wheelData = wheelDatas[i];
                wheelData.FrictionCurve.extremumSlip = wheelData.ExtremumSlip;
                wheelDatas[i] = wheelData;
                wheels[i].sidewaysFriction = wheelData.FrictionCurve;
            }
            driftingAxis = 0f;
        }
        
    }

    private void CheckHandBrake()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;
        if (handbrake)
        {
            isTractionLocked = false;
            driftingAxis += Time.fixedDeltaTime;
            float FL_ExtremumSlip = turnWheels[0].sidewaysFriction.extremumSlip; 
            float secureStartingPoint = driftingAxis * FL_ExtremumSlip * driftMultiplier;
            if (secureStartingPoint < FL_ExtremumSlip)
            {
                driftingAxis = FL_ExtremumSlip / (FL_ExtremumSlip * driftMultiplier);
            }
            if (driftingAxis > 1f)
            {
                driftingAxis = 1f;
            }
            if (driftingAxis < 1f)
            {
                for (int i = 0; i < wheels.Length; i++)
                {
                    WheelData wheelData = wheelDatas[i];
                    wheelData.FrictionCurve.extremumSlip = wheelData.ExtremumSlip * driftMultiplier * driftingAxis;
                    wheelDatas[i] = wheelData;
                    wheels[i].sidewaysFriction = wheelData.FrictionCurve;
                }
            }
            isTractionLocked = true;
        }
    }

    private void ApplyThrottle()
    {
        float motorTorque = throttle * motorTorqueCurve.Evaluate(speed) * diffGearing / driveWheels.Length;
        foreach (WheelCollider wheel in wheels)
        {
            wheel.brakeTorque = 0;
        }
        foreach (WheelCollider wheel in driveWheels)
        {
            if (wheel.isGrounded)
                wheel.motorTorque = motorTorque;
        }
    }
    
    private void Brakes()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.brakeTorque = brakeForce;
        }
    }

    private void HandleBoost()
    {
        if (isBoosting)
        {
            rb.AddForce(transform.forward * boostForce);
            if (boostParticles.Length > 0 && !boostParticles[0].isPlaying)
            {
                foreach (ParticleSystem boostParticle in boostParticles)
                {
                    boostParticle.Play();
                }
            }
        }
        else
        {
            if (boostParticles.Length > 0 && boostParticles[0].isPlaying)
            {
                foreach (ParticleSystem boostParticle in boostParticles)
                {
                    boostParticle.Stop();
                }
            }
        }
    }

    private void HandlePFX()
    {
        if (driftParticles.Length != 0)
        {
            if (isDrifting)
            {
                if (driftParticles[0].isStopped)
                {
                    for (int i = 0; i < driftParticles.Length; i++)
                    {
                        driftParticles[i].Play();
                    }
                }
            }
            else
            {
                if (driftParticles[0].isPlaying)
                {
                    for (int i = 0; i < driftParticles.Length; i++)
                    {
                        driftParticles[i].Stop();
                    }
                }
            }
        }

        if (skidTrails.Length != 0)
        {
            if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(speed) > 12f)
            {
                if (!skidTrails[0].emitting)
                {
                    for (int i = 0; i < skidTrails.Length; i++)
                    {
                        skidTrails[i].emitting = true;
                    }
                }
            }
            else
            {
                if (skidTrails[0].emitting)
                {
                    for (int i = 0; i < skidTrails.Length; i++)
                    {
                        skidTrails[i].emitting = false;
                    }
                }
            }
        }
    } 
    
    public void ResetPosition(Vector3 resetPos, Quaternion resetRot)
    {
        transform.position = resetPos;
        transform.rotation = resetRot;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void SetSteeringAngle(float angle)
    {
        steering = Mathf.Clamp(angle, -SteerAngle, SteerAngle);
    }

    public void Jump()
    {
        if (!IsGrounded) return;
        rb.velocity += transform.up * jumpForce;
    }
}
