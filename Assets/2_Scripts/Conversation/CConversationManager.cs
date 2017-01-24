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
	[SerializeField]GameObject _touchText;
	[SerializeField]List<GameObject> _objs = new List<GameObject>();
	[SerializeField]Text textBox;
	[SerializeField] List<CentenceData> _datasList = new List<CentenceData>();
	[SerializeField] float _centenceDelay = 0.2f;
	bool _isNowCoutine = false;
	bool _isFastBrif = false;
	int CentenceCount = 0;
	// Use this for initialization
	void Start () {
		_objs.Add(_touchText);
		_datasList = _datasList.OrderBy(x => x._countNumber).ToList();
		StartCoroutine(StartBreifing_Co());
		AppSound.instance.SE_MENU_KEYBOARD.Play();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.touchCount>0 || Input.GetMouseButtonDown(0))
		{
			//Debug.Log("Input");
			switch (currentPhase)
			{
				case BrifPhase.Wait :
					if(_isNowCoutine)return;
					 StartCoroutine(Centence_Co());
				break;
				case BrifPhase.Write :
					_isFastBrif = true;
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
		_isNowCoutine = true;
		yield return new WaitForSeconds(0.2f);
		currentPhase = BrifPhase.Write;
		_isFastBrif = false;
		foreach (GameObject item in _objs)
		{
			if(item.activeInHierarchy)
			item.SetActive(false);
		}
		_centenceDelay = 0.2f;
		string currentCentence = "";
		textBox.text = "";
		//문장수와 카운트수가 같다면 브리핑나가기
		if(IsEquelsCentencesCount())
		{
		StartCoroutine(GotoInGame());
		yield break;
		}
		yield return new WaitForSeconds(0.3f);
		//카운트수와 같은 이름의 오브젝트 찾기 
		GameObject obj = _objs.Where(x=>x.name.Equals(CentenceCount.ToString())).Select(x=>x).FirstOrDefault();
		if(null != obj)
		obj.SetActive(true);
		//문장가져오기 
		List<string> centences = new List<string>();
		centences = _datasList[CentenceCount].GetCentences();
		for (int i = 0; i < centences.Count; i++)
		{
			currentCentence += centences[i]+" _";
			yield return new WaitForSeconds(_centenceDelay);
			textBox.text = currentCentence;
			currentCentence = currentCentence.Remove(currentCentence.Length-1);
		}
		yield return null;
		currentPhase = BrifPhase.Wait;
		_touchText.SetActive(true);
		CentenceCount +=1;
		_isNowCoutine = false;

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
		return ((CentenceCount+1)==_datasList.Count)?true:false;
	}
	/// <summary>
	/// This function is called when the behaviour becomes disabled or inactive.
	/// </summary>
	void OnDisable()
	{
		AppSound.instance.fm.Stop("SE");
	}
}
