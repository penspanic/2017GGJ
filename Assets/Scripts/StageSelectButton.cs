using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StageSelectButton : MonoBehaviour
{
    public int stageNum;

    Image lockImage;
    Text stageNumText;

    void Awake()
    {
        lockImage = transform.FindChild("Image").GetComponent<Image>();
        stageNumText = transform.FindChild("Text").GetComponent<Text>();

        if(PlayerPrefsManager.instance.IsExist("lastClearedStageNum") == true)
        {
            int lastClearedStageNum = PlayerPrefsManager.instance.GetInt("lastClearedStageNum");
            if(stageNum <= lastClearedStageNum + 1)
            {
                lockImage.enabled = false;
                GetComponent<Button>().enabled = true;
                stageNumText.text = stageNum.ToString();
            }
            else
            {
                GetComponent<Button>().enabled = false;
                stageNumText.text = "";
            }
        }
        else
        {
            if(stageNum == 1)
            {
                GetComponent<Button>().enabled = true;
                stageNumText.text = stageNum.ToString();
                lockImage.enabled = false;
            }
        }
    }
}
