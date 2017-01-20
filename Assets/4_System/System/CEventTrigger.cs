using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CEventTrigger : MonoBehaviour{
	
	bool _isTouchable;
	static public CEventTrigger current;
	public event Action onPress;
	public event Action onVoidClick;
	public event Action onClick;
	public event Action onDoubleClick;

	/// <summary>
	/// Whether the collider is enabled and the widget can be interacted with.
	/// </summary>

	public bool isColliderEnabled
	{
		get
		{
			Collider c = GetComponent<Collider>();
			if (c != null) return c.enabled;
			Collider2D b = GetComponent<Collider2D>();
			return (b != null && b.enabled);
		}
	}


	public void OnPress ()
	{
		if (current != null || !isColliderEnabled) return;
		current = this;
		Debug.Log("PressTrigger");
		if(null != onPress)
		onPress();
		current = null;
	}
	public void OnClick ()
	{
		if (current != null || !isColliderEnabled) return;
		current = this;
		Debug.Log("ClickTrigger");
		if(null != onClick)
		onClick();
		current = null;
	}

	public void OnDoubleClick ()
	{
		if (current != null || !isColliderEnabled) return;
		current = this;
		if(null != onDoubleClick)
		onDoubleClick();;
		current = null;
	}
	public void OnVoidClick ()
	{
		if (current != null || !isColliderEnabled) return;
		current = this;
		if(null != onVoidClick)
		onVoidClick();;
		current = null;
	}

	public void TouchableSwitch()
    {
        _isTouchable = !_isTouchable;
    }
}
