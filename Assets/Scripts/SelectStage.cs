using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectStage : MonoBehaviour
{

    bool isSelected = false;

    void Awake()
    {
        FadeFilter.instance.FadeIn(new Color(1f, 1f, 1f, 0f), 1f);
    }

    public void OnStageButtonDown(int stageNum)
    {
        if (isSelected == true)
            return;
        isSelected = true;
        GameManager.instance.OnStageButtonDown(stageNum);

        FadeFilter.instance.FadeOut(Color.black, 1f);
        Invoke("ChangeScene", 1f);

    }

    void ChangeScene()
    {
        //SceneManager.LoadScene("Briefing");
        SceneManager.LoadScene("InGame");
    }
}
