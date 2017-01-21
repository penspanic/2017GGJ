using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDragEvent : MonoBehaviour {

	public ITEMSTATE _itemstate;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void OnDragStart()
	{
		Debug.Log("드래그시작 :"+ _itemstate);
		
	}
}
