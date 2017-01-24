using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace vietlabs.h2 {

internal class h2_Copy
{
	internal static void CopyName(GameObject go)
	{
		var name = Selection.activeGameObject.name;
		Debug.Log(name);
		EditorGUIUtility.systemCopyBuffer = name;
	}
	
	internal static void CopyHierarchyName(GameObject go)
	{
		var name = h2_Utils.GetHierarchyName(Selection.activeGameObject.transform);
		EditorGUIUtility.systemCopyBuffer = name;
		
		var arr = name.Split('/');
		var off = string.Empty;
		var str = arr[0];
		
		for (var i = 1; i < arr.Length; i++)
		{
			off = "\t" + off;
			str += "\n" + off + arr[i];
		}
		Debug.Log(str);
	}
}
}