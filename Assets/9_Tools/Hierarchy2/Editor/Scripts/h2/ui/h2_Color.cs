using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    internal enum h2_ColorMode
    {
        Simple = 1, // 1 color
        DarkLight = 2, // 2 colors (Dark / Light)
        Advanced = 6 // 6 colors (None / Focus / Lose Focus)
    }

    [Serializable]
    internal class h2_Color
    {
        //------------------------------- STATIC -----------------------------

        public static readonly Color PLAYMODE_TINT = new Color32(204, 204, 204, 255);

        public static readonly Color[] BG_COLOR =
        {
            new Color32(56, 56, 56, 255), // DARK
            new Color32(193, 193, 193, 255), // LIGHT

            // SELECTED
            new Color32(62, 95, 150, 255), // DARK
            new Color32(65, 125, 231, 255), // LIGHT

            // SELECTED NON-FOCUS
            new Color32(72, 72, 72, 255), // DARK
            new Color32(142, 142, 142, 255) // LIGHT
        };

        public static readonly Color[] LABEL_COLOR =
        {
            new Color32(176, 176, 176, 255), // DARK
            new Color32(0, 0, 0, 255), // LIGHT

            // SELECTED
            new Color32(255, 255, 255, 255), // DARK
            new Color32(255, 255, 255, 255), // LIGHT

            // SELECTED NON-FOCUS
            new Color32(255, 255, 255, 255), // DARK
            new Color32(255, 255, 255, 255) // LIGHT
        };

        public static readonly Color[] YELLOW =
        {
            new Color32(195, 195, 0, 255),
            new Color32(135, 135, 135, 255),
            new Color32(128, 128, 0, 255)
        };

        //static Color INDIE_ON		= new Color32(0,	 0,	  0, 255);
        //static Color INDIE_OFF		= new Color32(175, 175, 175, 255);
        //static Color INDIE_HALF		= new Color32(64,   64,  64, 255);

        //static Color PRO_ON			= new Color32(195, 195,   0, 255);
        //static Color PRO_OFF		= new Color32(135, 135, 135, 255);
        //static Color PRO_HALF		= new Color32(128, 128,   0, 255);

        static readonly int[] COLOR_INDEXES =
        {
            1, 5, 3, 0, 4, 2
        };

        static readonly string[] COLOR_NAMES =
        {
            "Light",
            "Light Lose Focus",
            "Light Selected",
            "Dark",
            "Dark Lose Focus",
            "Dark Selected"
        };

        public static Color currentColor;
        public List<Color> colors;
        public h2_ColorMode mode;

        public h2_Color(Color[] pcolors)
        {
            mode = (h2_ColorMode) pcolors.Length;
            colors = new List<Color>();
            colors.AddRange(pcolors);
        }

        internal void resize(int n)
        {
            if (colors == null) colors = new List<Color>();

            var count = colors.Count;
            var delta = n - count;
            if (delta <= 0) return;

            for (var i = 0; i < delta; i++)
            {
                if (count > 0)
                {
                    colors.Add(colors[i%count]);
                }
                else
                {
                    colors.Add(Color.black);
                }
            }
        }

        public Color GetColor(bool selected = false, bool dark = true, bool focus = true, bool playMode = false)
        {
            if (colors.Count < (int) mode) resize((int) mode);

            if (mode == h2_ColorMode.Simple) return colors[0];
            if (mode == h2_ColorMode.DarkLight) return colors[dark ? 0 : 1];

            switch (mode)
            {
            case h2_ColorMode.Simple        : return colors[0];
            case h2_ColorMode.DarkLight     : return colors[dark ? 0 : 1];
            case h2_ColorMode.Advanced      : return colors[(dark ? 0 : 1) | ((selected ? focus ? 1 : 2 : 0) << 1)];
            }

            Debug.LogWarning("Unsupported color mode <" + mode + ">");
            return colors[0];
        }

        // InspectorGUI support
        internal void DrawInspector(Action<Rect, int, Color> drawer)
        {
            var h = 18f;
            var o = 5f;
            var oy = 2f;
            var ratio = 0.75f;

            var mRect = GUILayoutUtility.GetRect(0, Screen.width, h, h);
            var mWidth = mRect.width;

            if (drawer != null)
            {
                mRect.width *= ratio;
                drawer(mRect, -1, Color.black);

                mRect.x += ratio*mWidth + o;
                mRect.width = mWidth*(1 - ratio) - o;
            }

            var newMode = (h2_ColorMode) EditorGUI.EnumPopup(mRect, mode);
            if (newMode != mode)
            {
                mode = newMode;
                resize((int) mode);
            }

            for (var i = 0; i < COLOR_NAMES.Length; i++)
            {
                //swap index 
                var idx = COLOR_INDEXES[i];

                var rect = GUILayoutUtility.GetRect(0, Screen.width, h, h);
                var r = new Rect(rect.x, rect.y, rect.width*ratio, h);
                h2_GUI.SolidColor(r, BG_COLOR[idx]);
                r.x += o;
                r.y += oy;
                h2_GUI.LabelColor(r, COLOR_NAMES[i], LABEL_COLOR[idx]);
                r.x -= o;

                var color = colors[idx%(int) mode];

                if (drawer != null)
                {
                    drawer(r, idx, color);
                }

                r.x += ratio*rect.width + o;
                r.width = (1 - ratio)*rect.width - o;

                if (idx < (int) mode)
                {
                    DrawColorPicker(r, ref color);
                    colors[idx] = color;
                }
            }
        }

        //--------------------------------- UTILS -------------------------------

        public static Color[] GetHSBColors(float s = 0.65f, float a = 1f, float b = 1f)
        {
            return new[]
            {
                FromHSB(0.0f, s, b, a),
                FromHSB(0.1f, s, b, a),
                FromHSB(0.2f, s, b, a),
                FromHSB(0.3f, s, b, a),
                FromHSB(0.4f, s, b, a),
                FromHSB(0.5f, s, b, a),
                FromHSB(0.6f, s, b, a),
                FromHSB(0.7f, s, b, a),
                FromHSB(0.8f, s, b, a),
                FromHSB(0.9f, s, b, a)
            };
        }

        public static int GetColorIndex(bool select, bool focus, bool dark)
        {
            return (dark ? 0 : 1) | ((select ? focus ? 1 : 2 : 0) << 1);
        }

        public static Color GetBGColor(bool select, bool focus, bool dark)
        {
            var result = BG_COLOR[GetColorIndex(select, focus, dark)];
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return result*PLAYMODE_TINT;
            }

            return result;
        }

        public static Color FromHSB(float hh, float ss, float bb, float aa = 1f)
        {
            var r = bb;
            var g = bb;
            var b = bb;
            if (ss != 0)
            {
                var max = bb;
                var dif = bb*ss;
                var min = bb - dif;

                var h = hh*360f;

                if (h < 60f)
                {
                    r = max;
                    g = h*dif/60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f)*dif/60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f)*dif/60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f)*dif/60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f)*dif/60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f)*dif/60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), aa);
        }

        public static bool DrawColorPicker(Rect r, ref Color color)
        {
            var bw = 20f;
            var rb = new Rect(r.x, r.y, bw, r.height);
            if (GUI.Button(rb, "P", h2_GUI.miniButton))
            {
                color = currentColor;
                //currentColor = color;
                return true;
            }

            bw -= 2;
            var c = EditorGUI.ColorField(new Rect(r.x + bw, r.y, r.width - bw, r.height), color);
            if (c != color)
            {
                color = c;
                currentColor = c;
                return true;
            }
            return false;
        }
    }
}