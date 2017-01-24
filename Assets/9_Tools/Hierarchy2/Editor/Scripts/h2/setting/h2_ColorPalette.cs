using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;


namespace vietlabs.h2 {


[Serializable] public class h2_ColorPalette 
{
	const int nColors = 10;
	
	public bool showPalette;
	
	//public Color current;
	[SerializeField] public h2_ColorList[] list;
	
	public int recentIdx;
	public Color[] recentList; 
	
	internal void ResetDefault()
	{
		list = new []
		{
			new h2_ColorList(){ colors = h2_Color.BG_COLOR },
			new h2_ColorList(){ colors = h2_Color.GetHSBColors() }
		};
		
		h2_Color.currentColor = list[1].colors[0];
		
		#if H2_DEV
		Debug.Log("RESET COLOR PALETTE");
		#endif
	}
	
	internal void Save(Color c)
	{
		if (recentList == null || recentList.Length != nColors) {
			recentIdx = -1;
			recentList = new Color[nColors];
		}
		
		recentIdx = (recentIdx + 1) % nColors;
		recentList[recentIdx] = c;
	}
	
	public void Draw(){
		if (list == null || list.Length == 0)
		{
			#if H2_DEV
			Debug.LogWarning(this + " not ready !");
			#endif
			//ResetDefault();
			return;
		}
		
		EditorGUILayout.BeginHorizontal();
		{
            h2_Color.currentColor = EditorGUILayout.ColorField(h2_Color.currentColor);
            //current = EditorGUILayout.ColorField(current);
            showPalette = h2_GUI.DrawBanner(showPalette, "COLOR");
		}
		EditorGUILayout.EndHorizontal();
		
		if (showPalette)
		{
			for (var i = 0;i < list.Length; i++)
			{
				DrawColors(list[i].colors);
			}	
			
			if (recentList !=null)
			{
				DrawColors(recentList);	
			}
		}
	}
	
	void DrawColors(Color[] colors)
	{
		var e = Event.current;
		const int h = 25;
		
		EditorGUILayout.BeginHorizontal();
		{
			var r = GUILayoutUtility.GetRect(0, Screen.width, h, h);
			var w = r.width / nColors;
			
			for (var j = 0; j < colors.Length; j++)
			{
				var c= colors[j];
				var rr = new Rect(r.x + w * j, r.y, w, r.height);
				
				h2_GUI.SolidColor(rr, c);
				if (e.type == EventType.mouseDown && e.button == 0 && rr.Contains(e.mousePosition))
				{
					h2_Color.currentColor = c;
					e.Use();
				}
			}
			
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}
}

[Serializable] public class h2_ColorList {
	public Color[] colors;
}
}