using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class FloatingInterface : MonoBehaviour
{
    public Camera mainCamera;
    public int layerMask = 3;
    public float maxPanelDistance = 5f, minPanelDistance = 3f;
    public GameObject floatingCanvas;
    void Start()
    {
        //layerMask = 1 << layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        //int layerMask = 8;
        Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * maxPanelDistance, Color.red);
        if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, maxPanelDistance,  1 << layerMask))
        {
            

        }
        else
        {

        }
    }

    public void FollowCamera()
    {
        Vector3 followingPosition = mainCamera.transform.position + mainCamera.transform.forward * minPanelDistance;
        Quaternion followingRotation = mainCamera.transform.rotation;
        
        floatingCanvas.transform.position = followingPosition;
    }
}
