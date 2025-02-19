using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerPC : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;

    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private float lastFixedUpdateTime;
    
    
    [SerializeField] private float driftFactor = 0.9f; // Коэффициент бокового сцепления при заносе
    [SerializeField] private float gripFactor = 1f; // Обычное сцепление колес
    [SerializeField] private float driftThreshold = 10f; // Порог угла заноса, после которого начинается дрифт
    [SerializeField] private float counterSteerStrength = 3f; // Сила авто-контрруления


    private void FixedUpdate() {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        HandleDrift();
        StabilizeCar();
    }
    
    private void HandleDrift() {
        Rigidbody rb = GetComponent<Rigidbody>();

        // Рассчитываем направление движения относительно направления машины
        Vector3 velocityDirection = rb.velocity.normalized;
        float forwardDot = Vector3.Dot(transform.forward, velocityDirection);
        float rightDot = Vector3.Dot(transform.right, velocityDirection);
    
        float slipAngle = Mathf.Abs(Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg); // Угол заноса

        // Определяем, когда ослаблять сцепление
        bool isSliding = slipAngle > driftThreshold && rb.velocity.magnitude > 5f;

        WheelFrictionCurve rearFriction = rearLeftWheelCollider.sidewaysFriction;
    
        if (isSliding) {
            print("isSliding");
            rearFriction.stiffness = driftFactor; // Уменьшаем сцепление для дрифта
        } else {
            rearFriction.stiffness = gripFactor; // Восстанавливаем сцепление
        }

        rearLeftWheelCollider.sidewaysFriction = rearFriction;
        rearRightWheelCollider.sidewaysFriction = rearFriction;

        // Автоматическая корректировка угла поворота при дрифте
        if (isSliding) {
            float counterSteer = -rightDot * counterSteerStrength;
            currentSteerAngle += counterSteer;
        }
    }
    
    void StabilizeCar() {
        Rigidbody rb = GetComponent<Rigidbody>();

        float speedFactor = Mathf.Clamp(rb.velocity.magnitude / 50f, 0f, 1f); // Чем быстрее едем, тем сильнее стабилизация
        float stability = 5000f * speedFactor; 

        rb.AddForce(-transform.up * stability);
    }


    // private void LateUpdate()
    // {
    //     if (isBreaking)
    //     {
    //         var skid = FindObjectOfType<Skidmarks>();
    //         if (rearLeftWheelCollider.GetGroundHit(out WheelHit wheelHitInfo1))
    //         {
    //             Vector3 skidPoint = wheelHitInfo1.point + (GetComponent<Rigidbody>().velocity * (Time.time - lastFixedUpdateTime));
    //             skid.AddSkidMark(skidPoint, wheelHitInfo1.normal, 20f, _skidIndex);
    //             _skidIndex += 1;
    //             print("ok");
    //         }
    //         
    //         if (rearRightWheelCollider.GetGroundHit(out WheelHit wheelHitInfo2))
    //         {
    //             Vector3 skidPoint = wheelHitInfo2.point + (GetComponent<Rigidbody>().velocity * (Time.time - lastFixedUpdateTime));
    //             skid.AddSkidMark(skidPoint, wheelHitInfo2.normal, 1f, _skidIndex);
    //             _skidIndex += 1;
    //
    //         }
    //     }
    // }

    private int _skidIndex = 0;
    // WheelHit wheelHitInfo;
    private void GetInput() {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor() {
        rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
        rearRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
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