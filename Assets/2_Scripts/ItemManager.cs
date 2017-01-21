using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pattern;

public class ItemManager : Singleton<ItemManager> {

	ITEMSTATE _currentSelect;
	public ITEMSTATE currentSelect
	{
		get{return _currentSelect;}
	}

	public void turnItem(ITEMSTATE item)
	{
		_currentSelect = item;
	}

	public bool IsSelected()
	{
		return _currentSelect.Equals(ITEMSTATE.NONE)?false:true;
	}
	// Use this for initialization
	void Start () {
		_currentSelect = ITEMSTATE.NONE;
	}
}
