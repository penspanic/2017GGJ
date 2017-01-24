using System;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace vietlabs.h2
{
    [Serializable]
    public class h2_Setting : ScriptableObject
    {
        public static int RESET_STAMP = 1;


	
        static bool searched;
	    private static h2_Setting _current;
	    [SerializeField] internal h2_ParentIndicatorSettings ParentIndicator;
        [SerializeField] internal h2_ActiveIconSetting Active;
        [SerializeField] internal h2_CombineSetting Combine;
        [SerializeField] internal h2_CommonSetting Common;
        [SerializeField] internal h2_ComponentSetting Component;
        [SerializeField] internal h2_GOIconSetting GOIcon;
	public bool isActive;
        [SerializeField] internal h2_LayerSetting Layer;
        [SerializeField] internal h2_LockSetting Lock;
	
        [SerializeField] internal h2_ColorPalette palette;
        [SerializeField] internal h2_PrefabSetting Prefab;

	[SerializeField] internal bool previewAllStates;
	[SerializeField] internal h2_SceneViewHLSetting SceneViewHL;
	[SerializeField] internal h2_ScriptSetting Script;
	[SerializeField] internal h2_StaticSetting Static;
	[SerializeField] internal h2_TagSetting Tag;
	
        public static h2_Setting current
        {
            get
            {
                if (searched || _current != null) return _current;
	
                Search4Setting();
                return _current;
            }
        }

        void OnEnable()
        {
            if (_current == null)
            {
			_current = this;
			//Debug.LogWarning(Time.realtimeSinceStartup + " h2-setting : " + AssetDatabase.GetAssetPath(this) + " enabled !");
			h2_Utils.DelayRepaintHierarchy();
		}
		
		RESET_STAMP++;
		if (Common == null) Reset();
	}
	
	void Reset()
	{
		RESET_STAMP++;
		#if H2_DEV
		Debug.Log("RESET :  " + palette + "\n" + Component + "\n" + Common + "\n" + Active + "\n" + Static + "\n" + Combine + "\n" + Script + "\n" + Layer);
		#endif
		
		palette = new h2_ColorPalette();
		Common = new h2_CommonSetting();
		SceneViewHL = new h2_SceneViewHLSetting();
		Lock = new h2_LockSetting();
		Active = new h2_ActiveIconSetting();
		ParentIndicator = new h2_ParentIndicatorSettings();
		Static = new h2_StaticSetting();
		Combine = new h2_CombineSetting();
		GOIcon = new h2_GOIconSetting();
		Script = new h2_ScriptSetting();
		Tag = new h2_TagSetting();
		Layer = new h2_LayerSetting();
		Prefab = new h2_PrefabSetting();
		Component = new h2_ComponentSetting();
		
		palette.ResetDefault();
		Common.Reset();
		SceneViewHL.Reset();
		Lock.Reset();
		Active.Reset();
		ParentIndicator.Reset();
		Static.Reset();
		Combine.Reset();
		GOIcon.Reset();
		Script.Reset();
		Tag.Reset();
		Layer.Reset();
		Prefab.Reset();
		Component.Reset();
		
		EditorUtility.SetDirty(this);
		h2_Utils.DelayRepaintHierarchy();
	}
	
        public static h2_Setting Search4Setting()
        {
		searched = true;
		//Debug.Log("Search called !");
		
            var settings = h2_Asset.FindAssetOfTypeAll<h2_Setting>("Hierarchy2", ".asset");
            for (var i = 0; i < settings.Count; i++)
            {
                if (settings[i].isActive)
                {
				_current = settings[i];
				return _current;
			}	
		}
		
            if (settings.Count > 0)
            {
			_current = settings[0];
			settings[0].isActive = true;
			
			EditorUtility.SetDirty(settings[0]);
	            h2_Utils.DelaySaveAssetDatabase();
	            //AssetDatabase.SaveAssets();
		}
		
		return _current;
	}
	
        internal static void SaveColor(Color c)
        {
			if (_current == null) return;
            if (_current.palette == null)
	{
			_current.palette = new h2_ColorPalette();
		}
		_current.palette.Save(c);
	}
	
	//----------------- MENU -------------------
	
	    #if H2_DEV
	[MenuItem("Window/Hierarchy2/New H2-Setting")]
	    #endif
        public static void NewSetting()
        {
            var asset = CreateInstance<h2_Setting>();
			AssetDatabase.CreateAsset(asset, "Assets/H2-Setting.asset");
	        h2_Utils.DelaySaveAssetDatabase();
	        //AssetDatabase.SaveAssets();
	}
}

    [Serializable]
	internal class h2_Texture : h2_Color 
	{
		[SerializeField] internal Texture2D texture;
		
		public h2_Texture(Texture2D tex, Color[] colors) : base(colors)
		{
			texture = tex;
		}
		    
		public void Draw()
		{
			
			DrawInspector((Rect r, int idx, Color c)=>
			{
				if (idx ==-1)
				{
					r.height = 16f;
					texture = (Texture2D)EditorGUI.ObjectField(r, texture, typeof(Object), false);
					return;	
				}
				
				if (texture == null) return;
				h2_GUI.TextureColor(new Rect(r.x + r.width-18f, r.y, 16f, 16f), texture, c);
			});
		}
	}


    [Serializable]
    internal class h2_StateTexture
{
	const string MESSAGE_ICON_DISABLED = "Icon being disabled !";
	const string LABEL_CUSTOM_SELECTION_COLOR = "Custom selection color";
	const string LABEL_CUSTOM_PLAYMODE_COLOR = "Custom playMode color";
        //[SerializeField] internal bool playColor;
        //[SerializeField] internal bool selectColor;
	
	[SerializeField] internal int state;
	[SerializeField] internal h2_Texture[] states;
	
	internal void DrawStates(bool enable, string[] stateNames)
	{
		if (!enable)
		{
			EditorGUILayout.HelpBox(MESSAGE_ICON_DISABLED, MessageType.Warning);
			return;
		}
		
		//selectColor 	= GUILayout.Toggle(selectColor, LABEL_CUSTOM_SELECTION_COLOR);
		//playColor		= GUILayout.Toggle(playColor, LABEL_CUSTOM_PLAYMODE_COLOR);
		
		EditorGUILayout.BeginHorizontal();
		{
			//if (GUILayout.Button("RESET", h2_GUI.miniButton))
			//{
			//	if (reset != null) reset();	
			//	Event.current.Use();
			//}
			
			GUILayout.FlexibleSpace();
			
			state = GUILayout.Toolbar(state, stateNames);
		}
		EditorGUILayout.EndHorizontal();
		states[state].Draw();
	}
}
}