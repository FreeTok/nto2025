using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    bool logged = false;
    public FadeScript fade;
    public void Exit()
    {
        if (!logged)
        {
            fade.StartFade(-1);
            logged = true;
        }
        
    }
}
