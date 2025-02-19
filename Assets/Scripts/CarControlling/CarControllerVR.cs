using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CarControllerVR : MonoBehaviour
{
    public enum VehicleType
    {
        bus,
        truck,
        car
    }
    
    [SerializeField] private bool isVR = false;
    [SerializeField] private VehicleType vehicleType;
    
    private float horizontalInput, verticalInput, breakInput;
    private float currentSteerAngle, currentbreakForce;
    
    // Settings
    [Space, SerializeField] private float breakForce, maxSteerAngle;
    [SerializeField] private float[] motorForces; 
    private float _motorForce;

    [Space] [SerializeField] private GameObject speedArrow;

    [Space]
    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    [Space]
    // VR
    [SerializeField]
    private XRKnob wheel;
    [SerializeField]
    private XRGearBox2 gearBox;
    
    [SerializeField] private XRLever breakLever;
    
    [SerializeField] private InputActionReference m_gasInputAction, m_brakeInputAction;

    private int _currentGear;

    private float _currentSpeed = 0f;

    [SerializeField] private AudioSource motorAudio, reverseAudio;
    
    

    //private void OnEnable()
    //{
    //    m_gasInputAction.EnableDirectAction();
    //    m_brakeInputAction.EnableDirectAction();
    //}
    
    void Start()
    {
        if (gearBox != null)
        {
            // Подписываемся на событие изменения позиции
            gearBox.onPositionChanged.AddListener(OnLeverPositionChanged);
        }
    }

    void OnLeverPositionChanged(int newPosition)
    {
        print($"new pos - {newPosition}");
        _currentGear = newPosition;
        _motorForce = motorForces[_currentGear + 1];

        switch (_currentGear)
        {
            case -1:
                reverseAudio.Play();
                motorAudio.pitch = 1;
                break;
            case 0:
                reverseAudio.Stop();
                motorAudio.pitch = 0.5f;
                break;
            case 1:
                motorAudio.pitch = 1f;
                break;
            case 2:
                motorAudio.pitch = 0.75f;
                break;
            case 3:
                motorAudio.pitch = 0.6f;
                break;
        }
        
        print(_currentGear);
        print(_motorForce);
    }

    private void FixedUpdate() {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        UpdateSpeed();

        if (vehicleType == VehicleType.truck)
        {
            
        }
    }

    private void GetInput() {
        if (!isVR)
        {
            // Steering Input
            horizontalInput = Input.GetAxis("Horizontal");

            // Acceleration Input
            verticalInput = Input.GetAxis("Vertical");

            // Breaking Input
            breakInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }

        else
        {
            horizontalInput = 2 * wheel.value - 1;

            if (wheel.isSelected)
            {
                verticalInput = m_gasInputAction.action?.ReadValue<float>() ?? 0f;
                breakInput = m_brakeInputAction.action?.ReadValue<float>() ?? 0f;
            }
        }
    }

    private void HandleMotor() {
        if (!isVR)
        {
            frontLeftWheelCollider.motorTorque = verticalInput * motorForces[2];
            frontRightWheelCollider.motorTorque = verticalInput * motorForces[2];
            ApplyBreaking();
            return;
        }
        
        print(_motorForce);

        if (_currentGear == 1)
        {
            currentbreakForce = breakInput * breakForce;
            ApplyBreaking();

            return;
        }
        
        frontLeftWheelCollider.motorTorque = verticalInput * _motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * _motorForce;
        
        currentbreakForce = breakLever.value ? 1000f : breakInput * breakForce;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering() {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void UpdateSpeed()
    {
        _currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        speedArrow.transform.localRotation = Quaternion.Euler(0, 0f, -135f - _currentSpeed);
    }
}