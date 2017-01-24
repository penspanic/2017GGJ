using UnityEditor;
using UnityEngine;

using System.Linq;
using System.Collections.Generic;

namespace vietlabs.h2
{
public class h2_IconSetting : h2_FeatureSetting
{
        static readonly Color INDIE_ON = new Color32(0, 0, 0, 255);
        static readonly Color INDIE_OFF = new Color32(175, 175, 175, 255);
        static readonly Color INDIE_HALF = new Color32(64, 64, 64, 255);
	
        static readonly Color PRO_ON = new Color32(195, 195, 0, 255);
        static readonly Color PRO_OFF = new Color32(135, 135, 135, 255);
        static readonly Color PRO_HALF = new Color32(128, 128, 0, 255);
	
        static readonly Color[] activeColors =
        {
	        PRO_ON, INDIE_ON,	
		};
	
        static readonly Color[] inactiveColors =
        {
        	PRO_OFF,
			INDIE_OFF,	
		};
	
        static readonly Color[] halfColors =
        {
	        PRO_HALF,
	        INDIE_HALF
		};
	
        static readonly Color[][] stateColors = {activeColors, inactiveColors, halfColors};
	
	// --------------------------------------- INSTANCE --------------------------------------
	
	
	[SerializeField] internal h2_StateTexture stateTexture;
	
        public override bool isReady
        {
            get { return stateTexture != null && stateTexture.states != null && stateTexture.states.Length > 0; }
        }

	internal h2_Texture[] GetH2Textures(string[] iconNames, Color[][] themeColors = null)
	{
		if (themeColors == null) themeColors = stateColors;
		
		var list = new List<h2_Texture>();
        for (var i = 0; i < iconNames.Length; i++)
        {
			var icoName = iconNames[i];
			
			Texture2D tex = null;
			
			if (!string.IsNullOrEmpty(icoName))
			{
            	tex = h2_Asset.FindAssetOfType<Texture2D>(iconNames[i], "Hierarchy2", ".png")
                          ?? EditorGUIUtility.whiteTexture;
			}
	            
	        list.Add(new h2_Texture(tex, themeColors[i % themeColors.Length]));
		}
		return list.ToArray();
	}
	
	public void DrawIcon(Rect r, int idx, GameObject go)
	{
		if (Event.current.type != EventType.repaint) return;
		
		var s = stateTexture.states[idx];
		if (s.texture == null) return;
		
		var c = s.GetColor(
			h2_Selection.Contains(go),
			EditorGUIUtility.isProSkin,
			h2_Lazy.isFocus,
			h2_Lazy.isPlaying
		);
		
		h2_GUI.TextureColor(r, s.texture, c);
	}
}
}