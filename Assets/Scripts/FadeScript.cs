using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeScript : MonoBehaviour
{
    public float fadeTime = 1f;
    int sc = 0;
    float curTime = 0;
    bool logged = false, ff = false;
    public Image fadf;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (logged)
        {
            if (!ff)
            {
                ff = true;
                
            }
            
            if (curTime < fadeTime)
            {
                curTime += Time.deltaTime;
                fadf.color = new Vector4(0, 0, 0, curTime / fadeTime);
            }
            else
            {
                //SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sc));
                if (sc >= 0)
                    SceneManager.LoadScene(sc);
                else
                    Application.Quit();
            }

        }
    }

    public void StartFade(int sceneNum)
    {
        sc = sceneNum;
        logged = true;
    }
}
