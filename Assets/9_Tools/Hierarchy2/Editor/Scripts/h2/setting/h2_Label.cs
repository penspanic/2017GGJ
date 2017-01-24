using System;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    internal enum h2_LabelStyle
    {
        Label = 1,
        LabelColor = 2,

        Bg = 1 << 4,
        BgColor = 2 << 4,

        Label_w_Bg = Label | Bg,
        LabelColor_w_Bg = LabelColor | Bg,
        Label_w_BgColor = Label | BgColor
    }


    [Serializable]
    internal class h2_Label
	{
		[NonSerialized] public float maxWidth;
		
		public bool shortenName;
        public float align; // 0-1 : Left 2 Right

        public Color bgColor;
        public Color[] colors;
        public Color lbColor;
        public float padding;
        public h2_LabelStyle style;

        internal float DrawLabel(Rect r, string label, int c)
        {
            if (!h2_Lazy.isRepaint) return 0;
	        
            var hasBG = (int) style >> 4 > 0;
            var hasLabel = (style & (h2_LabelStyle.Bg - 1)) > 0;

            var w = hasLabel ? h2_GUI.GetMiniLabelWidth(label) + padding : 4;
            var drawRect = new Rect(r.x + align*(r.width - w), r.y, w, r.height);

            if (hasBG)
            {
                var singleColor = (colors.Length == 0) || ((style & h2_LabelStyle.Bg) > 0);
                h2_GUI.SolidColor(drawRect, singleColor ? bgColor : colors[c%colors.Length]);
            }

            if (hasLabel)
            {
                var singleColor = (colors.Length == 0) || ((style & h2_LabelStyle.Label) > 0);
                h2_GUI.MiniLabelColor(drawRect, label, singleColor ? lbColor : colors[c%colors.Length]);
            }

            return w;
        }

	    internal void DrawInspector(Func<int, string> getLabel, int count = -1)
		{
			GUILayout.BeginHorizontal();
			{
				if (h2_GUI.Toggle(ref shortenName, "Shorten Name")){
					maxWidth = 0;
				};
				GUILayout.Label("Align", GUILayout.Width(50f));
				align = GUILayout.HorizontalSlider(align, 0, 1f);
			}
			GUILayout.EndHorizontal();
			
            GUILayout.BeginHorizontal();
            {
                var style1 = (h2_LabelStyle) EditorGUILayout.EnumPopup(style);
                if (style != style1)
                {
                    style = style1;
                    h2_Layer.dirtyMaxWidth = true;
                }

                var rr = GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);
                rr.width /= 2f;
                h2_Color.DrawColorPicker(rr, ref lbColor);

                rr.x += rr.width;
                h2_Color.DrawColorPicker(rr, ref bgColor);
            }
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);
	        
	        if (count == -1) count = colors.Length;
	        for (var i = 0; i < count; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    var r = GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);
                    r.width = r.width/2f;
	                
	                var lb = getLabel(i);
	                if (lb != null) { // empty : still draw, only skip drawing when return string is null !
	                	DrawLabel(r, getLabel(i), i);	
	                }
	                
                    r.x += r.width;
                    h2_Color.DrawColorPicker(r, ref colors[i]);
                }
                GUILayout.EndHorizontal();
            }

            h2_GUI.DrawLine();
        }
    }
}