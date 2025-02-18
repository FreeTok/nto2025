using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

public class DroneSimpleControl : MonoBehaviour
{
    //======================== SERIALIZABLE VALUES =======================================

    [SerializeField] private Vector3 velocityLimit, maxAppliedForce;
    [SerializeField] private float brakingForceMultiplier = 2f;
    [Range(0.0f, 1.1f)]
    [SerializeField] private float brakingSmoothMultiplier = 0.7f;
    [SerializeField] private float idleForceMultiplier = 1f;
    [SerializeField] private float convertCoefficient = 1f;
    public bool reverseJoysticks = false;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 180;
    [SerializeField] private bool enablePhysicsRotation = true;
    [SerializeField] private Vector3 angularVelocityLimit;

    [Header("Animation")]
    [SerializeField] private bool animationEnabled = true;
    [SerializeField] private Transform model;
    [SerializeField] private Vector3 maxRotation;
    [SerializeField] private float smooth = 0.8f;

    [Header("Extras")]
    private float theoRot;
    [SerializeField] private float physSmoothMultiplier = 0f;
    //======================================== START ===============================================

    private Rigidbody body;
    private Vector3 appliedForce = Vector3.zero, appliedAngularForce = Vector3.zero;

    private void Start()
    {
        body = GetComponent<Rigidbody>();

        enablePhysicsRotation = false;

        if (body == null) throw new ArgumentNullException("Rigidbody not found");
        if (model == null) animationEnabled = false;

        theoRot = transform.rotation.y;
    }

    //======================================== MAIN METHODS =========================================

    /*private void Update()
    {
        Vector2 lInput, rInput;
        if (!reverseJoysticks) // left - up/down/rotate, right - forward/backward/sideward
        {
            lInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick) * convertCoefficient;
            rInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick) * convertCoefficient;
        }
        else
        {
            rInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick) * convertCoefficient;
            lInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick) * convertCoefficient;
        }

        float strafeInput = -lInput.x,
              forwardInput = -lInput.y,
              rotateInput = rInput.x,
              highInput = rInput.y;

        Vector3 currentVelosity = transform.InverseTransformDirection(body.velocity), appliedForce_now = Vector3.zero;

        appliedForce_now.x = getForce(strafeInput, currentVelosity.x, maxAppliedForce.x);
        appliedForce_now.y = getForce(highInput, currentVelosity.y, maxAppliedForce.y);
        appliedForce_now.z = getForce(forwardInput, currentVelosity.z, maxAppliedForce.z);

        appliedForce = appliedForce_now;

        if (enablePhysicsRotation)
        {
            theoRot += Mathf.Pow(rotateInput, 2) * rotateSpeed;
            Quaternion theor = new Quaternion(0, theoRot, 0, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, theor, physSmoothMultiplier);

        }
        else
        {
            transform.Rotate(0, rotateInput * rotateSpeed * Time.deltaTime, 0);
        }
    }*/

    private void FixedUpdate()
    {
        if (animationEnabled) AnimationByForce(appliedForce);

        //body.AddRelativeForce(appliedForce-Physics.gravity*body.mass*idleForceMultiplier, ForceMode.Force);
        body.AddRelativeForce(appliedForce, ForceMode.Force);
        Vector3 limitedVelocity = Vector3.zero, currentVelosity = transform.InverseTransformDirection(body.velocity);

        limitedVelocity.x = limitAxis(currentVelosity.x, velocityLimit.x);
        limitedVelocity.y = limitAxis(currentVelosity.y, velocityLimit.y);
        limitedVelocity.z = limitAxis(currentVelosity.z, velocityLimit.z);

        body.velocity = transform.TransformDirection(limitedVelocity);
    }

    //======================================== OTHER METHODS ========================================

    private void AnimationByForce(Vector3 appliedForce)
    {
        Vector3 rotation = Vector3.zero;

        rotation.x = Mathf.LerpAngle(appliedForce.z / velocityLimit.z * maxRotation.x, model.localRotation.eulerAngles.x, smooth);
        rotation.z = Mathf.LerpAngle(appliedForce.x / velocityLimit.x * -maxRotation.z, model.localRotation.eulerAngles.z, smooth);
        rotation.y = model.localRotation.eulerAngles.y;

        model.localRotation = Quaternion.Euler(rotation);
    }

    //======================================== SPEC METHODS =========================================
    public float GetMaxPower() => maxAppliedForce.magnitude * brakingForceMultiplier;
    private float getForce(float input, float velosity, float maxAppliedForceAxis) =>
        maxAppliedForceAxis * input +
        (input == 0 & velosity != 0 ? -(Mathf.Abs(velosity) / velosity) * (Mathf.Abs(velosity) < brakingSmoothMultiplier ? Mathf.Abs(velosity) : brakingSmoothMultiplier) * maxAppliedForceAxis * brakingForceMultiplier : 0);
    private float limitAxis(float currentNumber, float limit) =>
        (Mathf.Abs(currentNumber) > limit ? limit * (Mathf.Abs(currentNumber) / currentNumber) : currentNumber);
}
