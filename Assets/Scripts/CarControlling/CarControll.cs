using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CarControllerVR : MonoBehaviour
{
    [SerializeField] private bool isVR = false;
    
    private float horizontalInput, verticalInput, breakInput;
    private float currentSteerAngle, currentbreakForce;
    
    // Settings
    [Space, SerializeField] private float motorForce, breakForce, maxSteerAngle;
    
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
    private XRGearBox gearBox;
    
    [SerializeField] private InputActionReference m_gasInputAction, m_brakeInputAction;

    private int _currentGear;

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
        _currentGear = newPosition;
    }

    private void FixedUpdate() {
        //print(m_gasInputAction.action?.ReadValue<float>());
        
        
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
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
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce;
            ApplyBreaking();
            return;
        }
        
        
        switch (_currentGear)
        {
            case 0:
                breakForce = 1;
                print("parking");
                break;
            case 1:
                frontLeftWheelCollider.motorTorque = verticalInput * -motorForce;
                frontRightWheelCollider.motorTorque = verticalInput * -motorForce;
                break;
            case 2:
                frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
                frontRightWheelCollider.motorTorque = verticalInput * motorForce;
                break;
            case 3:
                print("N");
                break;
        }
        
        currentbreakForce = breakInput * breakForce;
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
}