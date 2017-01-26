using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CGotoLogoScene : MonoBehaviour {
    bool isSceneChanging = false;
    public void OnBackButtonDown()
    {
        if (isSceneChanging == true)
        {
            return;
        }
        isSceneChanging = true;

        AppSound.instance.SE_MENU_BUTTON.Play();

        FadeFilter.instance.FadeOut(Color.black, 1f);
        Invoke("GotoLogo", 1f);
    }

    void GotoLogo()
    {
        Debug.Log("Logo");
        SceneManager.LoadScene("Logo");
    }
}
