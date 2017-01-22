using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectStage : MonoBehaviour
{

    bool isSelected = false;
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
        SceneManager.LoadScene("Briefing");
    }
}
