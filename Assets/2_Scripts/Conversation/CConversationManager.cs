using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CConversationManager : MonoBehaviour {
	List<CentenceData> Datas = new List<CentenceData>();
	public float _cntenceDelay = 0.2f;
	int CentenceCount = 0;
	// Use this for initialization
	void Start () {
		Datas = GetComponentsInChildren<CentenceData>().ToList();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	IEnumerator Centence_Co()
	{
		string currentCentence = "";
		List<string> centences = new List<string>();
		centences = Datas[CentenceCount].GetCentences();
		for (int i = 0; i < centences.Count; i++)
		{
			
		}
		yield return null;
	}
}
