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
    public Quaternion targetRotation, panelRotation;
    public Vector3 eulerTargetRot, eulerPanelRot;
    public float angle;

    public bool followNow = false;

    void Start()
    {
        if (camera == null)
            camera = Camera.main.transform;
    }

    void Update()
    {
        targetPosition = camera.position + camera.forward * distanceFromCamera;
        //targetRotation = Quaternion.LookRotation(targetPosition - vrCamera.position);
        targetRotation = new Quaternion(camera.rotation.x * 0, camera.rotation.y, camera.rotation.z * 0, camera.rotation.w);
        panelRotation = new Quaternion(this.transform.rotation.x * 0, this.transform.rotation.y, this.transform.rotation.z * 0, this.transform.rotation.w);
        angle = Quaternion.Angle(panelRotation, targetRotation);
        //eulerPanelRot = this.transform.eulerAngles;
        //eulerTargetRot = camera.eulerAngles;
        //angle = Mathf.Sqrt(((camera.eulerAngles.z - this.transform.eulerAngles.z) % 360) * ((camera.eulerAngles.x - this.transform.eulerAngles.x) % 360) * 0 + ((camera.eulerAngles.y - this.transform.eulerAngles.y) % 360) * ((camera.eulerAngles.y - this.transform.eulerAngles.y) % 360));
        //angle = Quaternion.A
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
        transform.eulerAngles = new Vector3();
        Quaternion curRotation = Quaternion.Slerp(panelRotation, targetRotation, rotateSpeed * Time.deltaTime);
        transform.rotation = curRotation;
    }
}