using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CConversationManager : MonoBehaviour {
	enum BrifPhase
	{
		Ready,
		Wait,
		Write
	}
	BrifPhase currentPhase = BrifPhase.Ready;
	[SerializeField]GameObject _moneyObj;
	[SerializeField]GameObject _knifeObj;
	[SerializeField]GameObject _touchText;
	List<GameObject> _objs = new List<GameObject>();
	[SerializeField]Text textBox;
	[SerializeField] List<CentenceData> Datas = new List<CentenceData>();
	[SerializeField] float _centenceDelay = 0.2f;
	bool _isNowCoutine = false;

	int CentenceCount = 0;
	// Use this for initialization
	void Start () {
		_objs.Add(_moneyObj);
		_objs.Add(_knifeObj);
		_objs.Add(_touchText);
		StartCoroutine(StartBreifing_Co());
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.touchCount>0 || Input.GetMouseButtonDown(0))
		{
			Debug.Log("Input");
			switch (currentPhase)
			{
				case BrifPhase.Wait :
					if(_isNowCoutine)return;
					 StartCoroutine(Centence_Co());
				break;
				case BrifPhase.Write :
					_centenceDelay = 0.0f;
				break;
				default:
					Debug.Log("is null");
				break;
			}
		}
	}
	IEnumerator StartBreifing_Co()
	{
        FadeFilter.instance.FadeIn(Color.black, 1f);
		yield return new WaitForSeconds(2.0f);
		StartCoroutine(Centence_Co());
	}
	IEnumerator Centence_Co()
	{
		currentPhase = BrifPhase.Write;
		_isNowCoutine = true;
		foreach (GameObject item in _objs)
		{
			if(item.activeInHierarchy)
			item.SetActive(false);
		}
		_centenceDelay = 0.2f;
		string currentCentence = "";
		textBox.text = "";
		yield return new WaitForSeconds(1f);
		switch (CentenceCount)
		{
			case 0:
			break;
			case 1:
				_moneyObj.SetActive(true);
			break;
			case 2:
				_knifeObj.SetActive(true);
			break;
			case 3:
			break;
			case 4:
				StartCoroutine(GotoInGame());
				yield break;
			default:
			break;
		}
		List<string> centences = new List<string>();
		centences = Datas[CentenceCount].GetCentences();
		for (int i = 0; i < centences.Count; i++)
		{
			currentCentence += centences[i]+" ";
			yield return new WaitForSeconds(_centenceDelay);
			textBox.text = currentCentence;
		}
		yield return null;
		currentPhase = BrifPhase.Wait;
		_touchText.SetActive(true);
		CentenceCount +=1;
		_isNowCoutine = false;
		Debug.Log(CentenceCount);

	}

	IEnumerator GotoInGame()
	{
        FadeFilter.instance.FadeOut(Color.black, 1f);
		yield return new WaitForSeconds(1.2f);

        if(GameManager.instance.selectedStageNum == -1) // 다시 로고씬으로 돌아가야 할 때
        {
            SceneManager.LoadScene("Logo");
            yield break;
        }

		SceneManager.LoadScene("InGame");
	}

	bool IsEquelsCentencesCount()
	{
		return ((CentenceCount+1)==Datas.Count)?true:false;
	}
}
