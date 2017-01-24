using System;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Layer : h2_Icon
    {
        public static bool dirtyMaxWidth;

        //---------------------------- LAYER CACHE ----------------------------

        static string[] LayerNumbers;

        public h2_Layer() : base("h2_layer")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Layer;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Layer.Draw");
#endif

	        if (go == null || setting == null || !h2_Lazy.isRepaint)
            {
#if H2_DEV
			Profiler.EndSample();
#endif
                return 0;
            }
	        
	        var ss = setting as h2_LayerSetting;
            var drawRect = new Rect(r.x + r.width - MaxWidth, r.y, MaxWidth, r.height);
	        var ww = ss.DrawLayer(drawRect, go.layer);

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

        protected override void RunCommand(string cmd)
        {
            switch (cmd)
            {
                case h2_LayerSetting.CMD_SHOW_LAYER:
                {
                    setting.enableIcon = !setting.enableIcon;
	                EditorUtility.SetDirty(h2_Setting.current);
	                h2_Utils.DelaySaveAssetDatabase();
	                //AssetDatabase.SaveAssets();
                    return;
                }

                case h2_LayerSetting.CMD_APPLY_LAYER_CHILDREN:
                {
                    var o = Selection.activeGameObject;
                    if (o != null)
                    {
                        Undo.IncrementCurrentGroup();
                        SetLayerRecursive(o.transform, o.layer);
                    }
                    return;
                }
            }

            Debug.LogWarning("Unsupported command <" + cmd + ">");
        }

        // Cache to prevent GC allocated per call
        public static string GetLayerName(int layer, bool shorten)
        {
            var name = LayerMask.LayerToName(layer);

            if (string.IsNullOrEmpty(name))
            {
                if (LayerNumbers == null)
                {
                    LayerNumbers = new string[32];
                }

                name = LayerNumbers[layer];

                if (string.IsNullOrEmpty(name))
                {
                    name = "L." + layer;
                    LayerNumbers[layer] = name;
                }

                return name;
            }

            return shorten ? h2_Utils.GetShortenName(name) : name;
        }


        public static void SetLayerRecursive(Transform t, int layer)
        {
            if (layer != t.gameObject.layer)
            {
                Undo.RecordObject(t.gameObject, "Set Layer");
                t.gameObject.layer = layer;
            }

            foreach (Transform c in t)
            {
                if (c == t) continue;
                SetLayerRecursive(c, layer);
            }
        }
    }

    internal enum h2_LayerDisplay
    {
        Label,
        LabelColor,
        LabelColorWithBG,

        SolidColor,
        SolidColorWithLabel
    }


    [Serializable]
    internal class h2_LayerSetting : h2_FeatureSetting
    {
        internal const string CMD_SHOW_LAYER = "show_layer";
        internal const string CMD_APPLY_LAYER_CHILDREN = "apply_layer_children";

        const string TITLE = "LAYER";
        const string SHORTEN_LAYER_LABEL = "Shorten Layer Name";
        const string DEFAULT_LAYER_LABEL = "Show Default Layer";

        static readonly string[] SHORTCUTS =
        {
            "Show / Hide Layers", CMD_SHOW_LAYER, "#%&L",
            "Apply Layer to children", CMD_APPLY_LAYER_CHILDREN, string.Empty
        };

        public h2_Label labelColor;

        // ------------------- INSTANCE -----------------------


	    //public bool shortenName;
        public bool showDefaultlayer;
        //public float padding;
        //
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

	        //shortenName = true;
	        showDefaultlayer = true;

            //padding				= 4f;
            //style				= h2_LayerDisplay.LabelColor;
            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);


            var arr = h2_Color.GetHSBColors();
            var colors = new Color[32];
            for (var i = 0; i < 32; i++)
            {
                colors[i] = arr[i%arr.Length];
            }

            labelColor = new h2_Label
            {
	            padding = 0,
	            shortenName = true,
                align = 1,
                style = h2_LabelStyle.LabelColor,
                lbColor = new Color32(0, 128, 64, 255),
                bgColor = new Color32(0, 128, 64, 255),
                colors = colors
            };


#if H2_DEV
		Debug.Log("RESET LAYER");
#endif
        }

        internal float DrawLayer(Rect r, int layer)
        {
            if (!showDefaultlayer && (layer == 0)) return 0;
            if (!h2_Lazy.isRepaint) return 0;

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

	        return labelColor.DrawLabel(r, h2_Layer.GetLayerName(layer, labelColor.shortenName), layer);
        }

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                //if (h2_GUI.Toggle(ref shortenName, SHORTEN_LAYER_LABEL))
                //{
	            //    h2_Layer.dirtyMaxWidth = shortenName;
                //}

	            //showDefaultlayer = GUILayout.Toggle(showDefaultlayer, DEFAULT_LAYER_LABEL);
	            if (h2_GUI.Toggle(ref showDefaultlayer, DEFAULT_LAYER_LABEL)){
	            	labelColor.maxWidth = 0;
	            }

                labelColor.DrawInspector(idx =>
                {
                	if (idx == 0 && !showDefaultlayer) return null;
	                return h2_Layer.GetLayerName(idx, labelColor.shortenName);
                });

                //GUILayout.BeginHorizontal();
                //{
                //	var style1 = (h2_LayerDisplay) EditorGUILayout.EnumPopup(style);
                //	if (style != style1){
                //		style = style1;
                //		h2_Layer.dirtyMaxWidth = true;
                //	}

                //	h2_Color.DrawColorPicker(GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f), ref color);
                //}
                //GUILayout.EndHorizontal();


                //for (var i = 0;i < colors.Length; i++)
                //{
                //	if (i == 3 || i == 5 || i == 6 || i == 7) continue;

                //	GUILayout.BeginHorizontal();
                //	{
                //		var r= GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);
                //		r.width = r.width/2f;

                //		DrawLayer(r, i);

                //		r.x += r.width;
                //		h2_Color.DrawColorPicker(r, ref colors[i]);	
                //	}
                //	GUILayout.EndHorizontal();
                //}

                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}