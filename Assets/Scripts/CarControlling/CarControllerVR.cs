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
        car,
        driftCar
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
    
    [SerializeField] private List<WheelCollider> wheelColliders;
    [SerializeField] private List<Transform> wheelTransforms;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    
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

    [SerializeField] private AudioSource motorAudio, reverseAudio, klapanAudio, driftAudio;
    
    [SerializeField] private GameObject connectedIco, unconnectedIco;
    
    [SerializeField] private GameObject[] insideColliders;
    
    [SerializeField] private float driftFactor = 0.9f; // Коэффициент бокового сцепления при заносе
    [SerializeField] private float gripFactor = 1f; // Обычное сцепление колес
    [SerializeField] private float driftThreshold = 4f; // Порог угла заноса, после которого начинается дрифт
    [SerializeField] private float counterSteerStrength = 3f; // Сила авто-контрруления
    
    
    
    

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
        UpdateSpeed();
        UpdateWheels();


        if (vehicleType == VehicleType.driftCar)
        {
            HandleDrift();
            StabilizeCar();
        }
    }

    [SerializeField] private GameObject[] smokes;
    
    private void HandleDrift() {
        Rigidbody rb = GetComponent<Rigidbody>();

        // Рассчитываем направление движения относительно направления машины
        Vector3 velocityDirection = rb.velocity.normalized;
        float forwardDot = Vector3.Dot(transform.forward, velocityDirection);
        float rightDot = Vector3.Dot(transform.right, velocityDirection);
    
        float slipAngle = Mathf.Abs(Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg); // Угол заноса

        // Определяем, когда ослаблять сцепление
        bool isSliding = slipAngle > driftThreshold && rb.velocity.magnitude > 5f;

        WheelFrictionCurve rearFriction = wheelColliders[2].sidewaysFriction;
    
        if (isSliding) {
            if (!driftAudio.isPlaying)
            {
                driftAudio.Play();

            }
            print("isSliding");

            foreach (var smoke in smokes)
            {
                smoke.SetActive(true);
            }
            
            rearFriction.stiffness = driftFactor; // Уменьшаем сцепление для дрифта
        } 
        else {
            rearFriction.stiffness = gripFactor; // Восстанавливаем сцепление
            Invoke(nameof(DisableSmokes), 5);
        }

        wheelColliders[2].sidewaysFriction = rearFriction;
        wheelColliders[3].sidewaysFriction = rearFriction;

        // Автоматическая корректировка угла поворота при дрифте
        if (isSliding) {
            float counterSteer = -rightDot * counterSteerStrength;
            currentSteerAngle += counterSteer;
        }
    }

    private void DisableSmokes()
    {
        foreach (var smoke in smokes)
        {
            smoke.SetActive(false);
        }
        driftAudio.Stop();
    }
    
    void StabilizeCar() {
        Rigidbody rb = GetComponent<Rigidbody>();

        float speedFactor = Mathf.Clamp(rb.velocity.magnitude / 50f, 0f, 1f); // Чем быстрее едем, тем сильнее стабилизация
        float stability = 5000f * speedFactor; 

        rb.AddForce(-transform.up * stability);
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

        if (_currentGear == 0)
        {
            currentbreakForce = breakInput * breakForce;
            ApplyBreaking();

            return;
        }
        
        // print($"Vertical inpuit - {verticalInput}");

        if (verticalInput > 0)
        {
            Invoke(nameof(EnableKlapan), 1f);
        }

        else
        {
            klapanAudio.Stop();
        }
        
        foreach (var collider in insideColliders)
        {
            collider.SetActive(verticalInput <= 0);
        }
        
        for (int i = 0; i < wheelColliders.Count; i++)
        {
            // print($"{i} - {verticalInput * _motorForce}");
            wheelColliders[i].motorTorque = verticalInput * _motorForce;
        }
        
        currentbreakForce = breakLever.value ? 1000f : breakInput * breakForce;
        ApplyBreaking();
    }
    
    private void EnableKlapan()
    {
        klapanAudio.Play();
    }

    private void ApplyBreaking() {
        for (int i = 0; i < wheelColliders.Count; i++)
        {
            wheelColliders[i].brakeTorque = currentbreakForce;
        }
    }

    private void HandleSteering() {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() {
        for (int i = 0; i < wheelColliders.Count; i++)
        {
            UpdateSingleWheel(wheelColliders[i], wheelTransforms[i]);
        }
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        if (!wheelCollider || !wheelTransform) return; 
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

    public void ConnectTruck(TrailerController trailer)
    {
        trailer.GetComponent<HingeJoint>().connectedBody = GetComponent<Rigidbody>();
        
        foreach (var wheelCollider in trailer.wheelColliders)
        {
            wheelColliders.Add(wheelCollider);
        }
        
        foreach (var wheelTransform in trailer.wheels)
        {
            wheelTransforms.Add(wheelTransform);
        }
        
        connectedIco.SetActive(true);
        unconnectedIco.SetActive(false);
    }

    public void DisconnectTruck(TrailerController trailer)
    {
        trailer.GetComponent<HingeJoint>().connectedBody = null;
        foreach (var wheelCollider in trailer.wheelColliders)
        {
            wheelColliders.Remove(wheelCollider);
        }
        foreach (var wheelTransform in trailer.wheels)
        {
            wheelTransforms.Remove(wheelTransform);
        }
        
        connectedIco.SetActive(false);
        unconnectedIco.SetActive(true);
    }

    // public void Cones(bool isPhysics)
    // {
    //     Physics.IgnoreLayerCollision(LayerMask.NameToLayer("XRRIG"), LayerMask.NameToLayer("Cone"), isPhysics);
    // }
}