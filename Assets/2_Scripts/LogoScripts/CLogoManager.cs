using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CLogoManager : MonoBehaviour {

	bool _isStart=false;
	public GameObject _creditPanel;
	public GameObject _logoPanel;
	public float GotoSceneDelay = 1.0f;
	// Use this for initialization
	void Start () {
		StartCoroutine(StartLogo_Co());
	}
	
	public void PushStart()
	{
		
		if(_isStart)return;
		_isStart = true;
		AppSound.instance.SE_MENU_BUTTON.Play();
		StartCoroutine(PushStart_Co());
	}
	public void GotoSelectStage()
	{
		SceneManager.LoadScene("SelectStage");
	}
    public void TutorialButtonDown()
    {
        if(_isStart == true)
        {
            return;
        }
        _isStart = true;

        GameManager.instance.selectedStageNum = -1;

        FadeFilter.instance.FadeOut(Color.black, 1f);
        Invoke("GotoBriefing", 1f);
    }
    private void GotoBriefing()
    {
        SceneManager.LoadScene("Briefing");
    }

	public void TurnOnCredit()
	{
		AppSound.instance.SE_MENU_BUTTON.Play();
		_creditPanel.SetActive(true);
	}
	public void TurnOffCredit()
	{
		AppSound.instance.SE_MENU_BUTTON.Play();
		_creditPanel.SetActive(false);
	}

	IEnumerator PushStart_Co()
	{
		yield return new WaitForSeconds(GotoSceneDelay);
		FadeFilter.instance.FadeOut(Color.black,0.8f);
		yield return new WaitForSeconds(0.8f);
		GotoSelectStage();
	}
	IEnumerator StartLogo_Co()
	{
		FadeFilter.instance.FadeIn(Color.black,0.5f);
        if (SystemData.Instance._isViewLogo)
        {
            _logoPanel.SetActive(false);
            yield break;
        }
        else
            SystemData.Instance._isViewLogo = true;
        yield return new WaitForSeconds(1.5f);
		FadeFilter.instance.FadeOut(Color.black,0.5f);
		yield return new WaitForSeconds(0.5f);
		FadeFilter.instance.FadeIn(Color.black,0.5f);
		_logoPanel.SetActive(false);
	}
}
