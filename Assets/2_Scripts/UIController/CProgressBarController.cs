using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CProgressBarController : MonoBehaviour {

	public StageManager _stageManager;
	float _barValue=1.0f;
	public Image _backGround;
	public Image _frontBar;
	
	
	// Update is called once per frame
	void Update () {
		if (!(_stageManager.GetDeliveredRate().Equals(_barValue)))
		{
			_frontBar.fillAmount = _stageManager.GetDeliveredRate();
		}
	}
}
