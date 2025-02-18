using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;


public class FloatingInterfaceFin : MonoBehaviour
{
    public Transform camera; 
    public float followSpeed = 5f, rotateSpeed = 5f;
    public float distanceFromCamera = 1f;
    public float lowerAngleThreshold = 5f, lowerDistanceThreshold = 0.01f;
    public float upperAngleThreshold = 45f, upperDistanceThreshold = 0.5f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    bool followNow = false;

    void Start()
    {
        if (camera == null)
            camera = Camera.main.transform;
    }

    void Update()
    {
        targetPosition = camera.position + camera.forward * distanceFromCamera;
        //targetRotation = Quaternion.LookRotation(targetPosition - vrCamera.position);
        targetRotation = camera.rotation;

        float angle = Quaternion.Angle(this.transform.rotation, targetRotation);

        if (angle > upperAngleThreshold || Vector3.Distance(transform.position, targetPosition) > upperDistanceThreshold)
            followNow = true;
        if (followNow)
            Follow();
        if (angle < lowerAngleThreshold && Vector3.Distance(transform.position, targetPosition) < lowerDistanceThreshold)
            followNow = false;
    }

    public void Follow()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}