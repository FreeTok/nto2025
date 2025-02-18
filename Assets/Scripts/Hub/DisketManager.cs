using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DisketManager : MonoBehaviour
{
    public float animTime = 1f;
    float curTime = 0;
    public Vector3 startPoint, endPoint;
    public GameObject dsk;
    short loggedState= 0;
    public Vector3 startRotation;
    public FadeScript fadeScript;

    Vector3 ClampVec(Vector3 start, Vector3 end, float cur, float an)
    {
        return new Vector3(Mathf.Clamp(start.x, end.x, cur / an), Mathf.Clamp(start.y, end.y, cur / an), Mathf.Clamp(start.z, end.z, cur / an));
    }

    private void Update()
    {
        if (loggedState > 0)
        {
            if(curTime < animTime)
            {
                curTime += Time.deltaTime;
                dsk.transform.position =  ClampVec(startPoint, endPoint, curTime, animTime);
            }
            else if(loggedState == 1)
            {
                loggedState = 2;
                fadeScript.StartFade(dsk.GetComponent<DisketHolder>().sceneNum);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Disket" && loggedState == 0)
        {
            loggedState = 1;
            dsk = other.gameObject;
            dsk.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            dsk.transform.rotation.SetEulerAngles(startRotation);
            
        }
    }
}
