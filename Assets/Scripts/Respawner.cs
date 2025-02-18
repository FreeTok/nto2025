using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    Vector3 resPos;
    Quaternion resRot;
    void Start()
    {
        resPos = transform.position;
        resRot = transform.rotation;
    }
    public void Restore()
    {
        print("Respawned");
        this.transform.position = resPos;
        this.transform.rotation = resRot;
        this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Restore")
            Restore();
        //if (other.gameObject.GetComponent<Respawner>() != null)
        //    other.gameObject.GetComponent<Respawner>().Restore();
    }

}
