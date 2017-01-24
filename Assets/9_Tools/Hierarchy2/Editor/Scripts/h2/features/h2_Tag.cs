using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using UnityEditorInternal;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Tag : h2_Icon
    {
        // ----------------------------- TAG CACHE -----------------------------

	    static float stamp;
	    static internal readonly Dictionary<string, int> tag2Index	= new Dictionary<string, int>();
	    static internal readonly Dictionary<int, string> instId2Tag	= new Dictionary<int, string>();

        public h2_Tag() : base("h2_tag")
        {
        }

        public override void RefreshSettings()
        {
            var t = h2_Setting.current.Tag;
            setting = t;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Tag.Draw");
#endif

	        if (go == null || setting == null || !h2_Lazy.isRepaint)
            {
#if H2_DEV
			Profiler.EndSample();
#endif
                return 0;
            }
            
	        var ss = setting as h2_TagSetting;
            var drawRect = new Rect(r.x + r.width - MaxWidth, r.y, MaxWidth, r.height);
	        var ww = ss.DrawTag(drawRect, go);

	        if (MaxWidth < ww || ss.labelColor.maxWidth == 0)
            {
		        MaxWidth = ww;
		        ss.labelColor.maxWidth = ww;
            }

#if H2_DEV
		Profiler.EndSample();
#endif

            return ww;
        }

        // ----------------------------- SHORTCUT HANDLER -----------------------------

        protected override void RunCommand(string cmd)
        {
            var go = Selection.activeGameObject;
            if (go == null) return;

            switch (cmd)
            {
                case h2_TagSetting.CMD_ENABLE_TAG:
                {
                    setting.enableIcon = !setting.enableIcon;
	                EditorUtility.SetDirty(h2_Setting.current);
	                h2_Utils.DelaySaveAssetDatabase();
	                //AssetDatabase.SaveAssets();
                    return;
                }
                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }
        }
	    
	    internal const string Untagged = "Untagged";
	    
        internal static string GetTag(GameObject go)
        {
            var realTime = Time.realtimeSinceStartup;
	        if (realTime - stamp > 1f || stamp == 0f) RefreshTagCache(realTime);

            var instID = go.GetInstanceID();
            string result;

	        if (!instId2Tag.TryGetValue(instID, out result))
            {
		        result = go.CompareTag(Untagged) ? Untagged : go.tag;
                instId2Tag.Add(instID, result);
            }

            return result;
        }
	    
	    internal static void RefreshTagCache(float time)
	    {
	    	var arr = InternalEditorUtility.tags;
		    tag2Index.Clear();
		    
		    for (var i = 0;i < arr.Length; i++){
			    tag2Index.Add(arr[i], i);
		    }
		    
		    var changeDict = new Dictionary<int, string>();
		    foreach (var item in instId2Tag)
		    {
			    var g = (GameObject) EditorUtility.InstanceIDToObject(item.Key);
			    if (g == null) {
				    changeDict.Add(item.Key, null); // removed
				    continue; //same tag or removed GO
			    }
			    
			    if (g.CompareTag(item.Value)) continue;
			    changeDict.Add(item.Key, g.CompareTag(Untagged) ? Untagged : g.tag);
		    }
		    
		    if (changeDict.Count > 0){
			    foreach (var item in changeDict)
			    {
				    if (string.IsNullOrEmpty(item.Value)) 
				    {//GO destroyed
					    instId2Tag.Remove(item.Key);
					    continue;
				    }
				    
            			//tag changed
				    instId2Tag[item.Key] = item.Value;
			    }
		    }
		    
		    stamp = time;
	    }
    }
	
	

    //[Serializable] internal class h2_TagIcon
    //{
    //	public Texture icon;
    //	public string tagName;
    //}

    [Serializable]
    internal class h2_TagSetting : h2_FeatureSetting
    {
        internal const string CMD_ENABLE_TAG = "enable_tag";

        const string TITLE = "TAG";
        const string SHORTEN_TAG_LABEL = "Shorten Tag Name";

        static readonly string[] SHORTCUTS =
        {
            "Show Tag in Hierarchy", CMD_ENABLE_TAG, "#%&T"
        };

        public h2_Label labelColor;

        // ------------------- INSTANCE -----------------------


        //public float padding;
	    //public bool shortenName;

        //public Color color;
        //public Color[] colors;

        //public h2_LayerDisplay style;

        public override bool isReady
        {
            get { return labelColor != null; }
        }
	    
        internal override void Reset()
        {
	        enableIcon = false;
            enableShortcut = true;

	        //shortenName = false;
            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);

            var arr = h2_Color.GetHSBColors();
            var colors = new Color[32];
            for (var i = 0; i < 32; i++)
            {
                colors[i] = arr[i%arr.Length];
            }

            labelColor = new h2_Label
            {
	            align = 1,
	            shortenName = false,
	            style = h2_LabelStyle.Label_w_BgColor,
                lbColor = new Color32(0, 0, 0, 255),
                bgColor = new Color32(0, 128, 64, 128),
                colors = colors
            };

#if H2_DEV
		Debug.Log("RESET TAG");
#endif
        }

        internal float DrawTag(Rect r, GameObject go)
        {
	        if (go.CompareTag(h2_Tag.Untagged)) return 0;
	        if (!h2_Lazy.isRepaint) return 0;
	        
	        //var tags = InternalEditorUtility.tags;

            //var hasBG		= style != h2_LayerDisplay.LabelColor && style != h2_LayerDisplay.Label;
            //var hasLabel	= style != h2_LayerDisplay.SolidColor;

            //var label		= hasLabel ? h2_Layer.GetLayerName(layer, shortenName) : string.Empty;
            //var w			= (hasLabel ? h2_GUI.GetMiniLabelWidth(label) : 0f) + padding;
            //r.x += r.width-w; 
            //r.width = w;

            //if (hasBG)
            //{
            //	var bgColor = style == h2_LayerDisplay.LabelColorWithBG ? color : colors[layer];
            //	h2_GUI.SolidColor(r, bgColor);
            //}

            //if (hasLabel)
            //{
            //	var singleColor = style == h2_LayerDisplay.Label || style == h2_LayerDisplay.SolidColorWithLabel;
            //	var lbColor = singleColor ? color : colors[layer];
            //	h2_GUI.MiniLabelColor(r, label, lbColor);
            //	//Debug.Log(label);
            //}
	        
	        var tagName = h2_Tag.GetTag(go);
	        int tagIdx;
	        
	        if (!h2_Tag.tag2Index.TryGetValue(tagName, out tagIdx)){
	        	tagIdx = 0;
	        };
	        
	        return labelColor.DrawLabel(r, labelColor.shortenName ? h2_Utils.GetShortenName(tagName) : tagName, tagIdx);
        }

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
	            var tags = InternalEditorUtility.tags;
	            
                labelColor.DrawInspector(idx =>
                {
                	if (idx >= tags.Length) return string.Empty;
	                var s = tags[idx];
                    return labelColor.shortenName ? h2_Utils.GetShortenName(s) : s;
                }, tags.Length);

                //stateTexture.DrawStates(enableIcon, ACTIVE_STATES);
                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}