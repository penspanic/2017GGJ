using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace vietlabs.h2 {
	

internal class h2_Goto
{
	private static List<Transform> _pingList;
	
	internal static void GotoRoot(GameObject go)
	{
		var t = go.transform.parent;
		if (t == null) return;
		
		while (t.parent != null)
		{
			t = t.parent;
		}
		
		Selection.activeGameObject = t.gameObject;
		EditorGUIUtility.PingObject(t.gameObject);
	}
	
	internal static void GotoParent(GameObject go)
	{
		var p = go.transform.parent;
		if (p == null) return;
		
            //clear history when select other GO
		if (_pingList == null)
		{
			_pingList = new List<Transform>();
		}
		else if (_pingList.Count > 0)
		{
                //Check & clear history if selection changed !
			var last = _pingList[_pingList.Count - 1];
			if (last.parent != go.transform) _pingList.Clear();
		}
		
		_pingList.Add(go.transform);
		Selection.activeGameObject = p.gameObject;
		EditorGUIUtility.PingObject(p.gameObject);
	}
	
	internal static void GotoSibling(GameObject go)
	{
		    //Debug.Log("Goto Next Sibling !");
		
		var siblings = h2_Unity.GetSiblings(go);
		if (siblings.Count <= 1) return;
		
		var idx = siblings.IndexOf(go);
		var next = siblings[(idx + 1 + siblings.Count)%siblings.Count];
		
		Selection.activeGameObject = next;
		EditorGUIUtility.PingObject(next);
	}
	
	internal static void GotoChild(GameObject go)
	{
		var t = go.transform;
		if (t.childCount == 0) return;
		
		Transform pingT = null;
		if (_pingList == null) _pingList = new List<Transform>();
		
		if (_pingList.Count > 0)
		{
			var idx = _pingList.Count - 1;
			var c = _pingList[idx];
			_pingList.Remove(c);
			
			if (c.parent == t)
			{
				pingT = c;
			}
			else
			{
#if H2_DEV
				Debug.LogWarning("Selction or Hierarchy changed !");
#endif
			}
		}
		
		if (pingT == null)
		{
			pingT = t.GetChild(0);
		}
		
		Selection.activeGameObject = pingT.gameObject;
		EditorGUIUtility.PingObject(pingT.gameObject);
	}
}
}