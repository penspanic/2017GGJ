
using UnityEngine;
using System.Collections;

public enum FOXFADE_STATE
{
	NON,
	IN,
	OUT,
};

public class FadeFilter : MonoBehaviour {

	public static FadeFilter instance = null;

	// === 외부 파라미터(Inspector 표시) =====================
	public GameObject 		fadeFilterObject 	= null;
	public string			attacheObject		= "FadeFilterPoint";

	// === 외부 파라미터 ======================================
	[System.NonSerialized] public FOXFADE_STATE	fadeState;

	// === 내부 파라미터 ======================================
	private float 			startTime;
	private float 			fadeTime;
	private Color 			fadeColor;

	// === 코드(Monobehaviour 기본 기능 구현) ================
	void Awake () {
		instance  = this;
		fadeState = FOXFADE_STATE.NON;
	}

	void SetFadeAction(FOXFADE_STATE state,Color color,float time) {
		fadeState = state;
		startTime = Time.time;
		fadeTime  = time;
		fadeColor = color;
	}

	public void FadeIn(Color color,float time) {
		SetFadeAction (FOXFADE_STATE.IN, color, time);
	}

	public void FadeOut(Color color,float time) {
		SetFadeAction (FOXFADE_STATE.OUT, color, time);
	}

	void SetFadeFilterColor(bool enbaled ,Color color) {
		if (fadeFilterObject) {
			fadeFilterObject.GetComponent<Renderer>().enabled 		 = enbaled;
			fadeFilterObject.GetComponent<Renderer>().material.color = color;
			SpriteRenderer sprite = fadeFilterObject.GetComponent<SpriteRenderer>();
			if (sprite) {
				sprite.enabled = enbaled;
				sprite.color   = color;
				fadeFilterObject.SetActive(enbaled);
			}
		}
	}
	
	void Update () {
		// 페이드 필터를 적용한다(씬 사이를 이동할 때에 대응한 처리)
		if (attacheObject != null) {
			GameObject go = GameObject.Find (attacheObject);
			fadeFilterObject.transform.position = go.transform.position;
		}
		// 페이드 처리
		switch(fadeState) {
		case FOXFADE_STATE.NON :
			break;
			
		case FOXFADE_STATE.IN :
			fadeColor.a = 1.0f - ((Time.time - startTime) / fadeTime);
			if (fadeColor.a > 1.0f || fadeColor.a < 0.0f) {
				fadeColor.a = 0.0f;
				fadeState = FOXFADE_STATE.NON;
				SetFadeFilterColor(false,fadeColor);
				break;
			}
			SetFadeFilterColor(true,fadeColor);
			break;

		case FOXFADE_STATE.OUT :
			fadeColor.a = (Time.time - startTime) / fadeTime;				
			if (fadeColor.a > 1.0f || fadeColor.a < 0.0f) {
				fadeColor.a = 1.0f;
				fadeState = FOXFADE_STATE.NON;
			}
			SetFadeFilterColor(true,fadeColor);
			break;
		}
		// Debug.Log (string.Format ("[FoxFadeFilter] fadeState:{0} fadeColor:{1},fadeTime:{2}", fadeState, fadeColor,fadeTime));
	}
}
