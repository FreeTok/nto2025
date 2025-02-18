using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnerManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Respawner>() != null)
            other.gameObject.GetComponent<Respawner>().Restore();
    }
}
