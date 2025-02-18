using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [SerializeField] private string tag;
    [SerializeField] private UnityEvent onTriggerEnter, onTriggerStay, onTriggerExit;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == tag)
        {
            onTriggerEnter.Invoke();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == tag)
        {
            onTriggerExit.Invoke();
        }
    }
}
