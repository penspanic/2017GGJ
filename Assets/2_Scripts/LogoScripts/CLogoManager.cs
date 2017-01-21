using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CLogoManager : MonoBehaviour {

	bool _isStart=false;
	public GameObject _creditPanel;
	public float GotoSceneDelay = 1.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void PushStart()
	{
		if(_isStart)return;
		_isStart = true;
		StartCoroutine(PushStart_Co());
	}
	public void GotoBriefing()
	{
		Debug.Log("브리핑룸으로");
	}
	public void TurnOnCredit()
	{
		_creditPanel.SetActive(true);
	}
	public void TurnOffCredit()
	{
		_creditPanel.SetActive(false);
	}
	IEnumerator PushStart_Co()
	{
		yield return new WaitForSeconds(GotoSceneDelay);
		FadeFilter.instance.FadeOut(Color.black,0.8f);
		yield return new WaitForSeconds(0.8f);
		GotoBriefing();
	}
}
