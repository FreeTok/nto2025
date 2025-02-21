using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSteer : MonoBehaviour
{
    public float speed = 1f;
    void Update()
    {
        transform.localEulerAngles += new Vector3(0, speed * Time.deltaTime, 0);
    }
}
