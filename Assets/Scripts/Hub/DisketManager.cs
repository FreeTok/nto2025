using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


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
        return  start + new Vector3((end.x - start.x) * Mathf.Clamp01(cur/an), (end.y - start.y) * Mathf.Clamp01(cur / an), (end.z - start.z) * Mathf.Clamp01(cur / an));
    }

    private void Update()
    {
        if (loggedState > 0)
        {
            if(curTime < animTime)
            {
                curTime += Time.deltaTime;
                dsk.transform.localPosition = ClampVec(startPoint, endPoint, curTime, animTime);
                //print(curTime.ToString() + ' ' + animTime.ToString());
                //Debug.Log(dsk.transform.localPosition);
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
            dsk.GetComponent<XRGrabInteractable>().enabled = false;
            dsk.transform.SetParent(this.transform);
            dsk.GetComponent<Rigidbody>().isKinematic = true;
            this.gameObject.GetComponent<AudioSource>().Play();
            //dsk.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            dsk.transform.rotation = new Quaternion(0, 0, 0, 0);
            
        }
    }
}
