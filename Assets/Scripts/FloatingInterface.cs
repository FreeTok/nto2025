using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingInterface : MonoBehaviour
{
    public FadeScript fader;
    public void CloseInterface()
    {
        this.gameObject.SetActive(false);
    }
    public void RestartScene()
    {
        fader.StartFade(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoToHub()
    {
        fader.StartFade(1);
    }
}
