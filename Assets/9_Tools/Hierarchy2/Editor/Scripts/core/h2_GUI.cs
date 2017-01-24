using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.h2
{
    public class h2_GUI
    {
        private static GUIStyle _miniLabel;

        private static GUIStyle _miniButton;

        static readonly Dictionary<string, int> miniLabelWCache = new Dictionary<string, int>();

        static readonly Color LINE_COLOR_LIGHT = new Color32(128, 128, 128, 255);
        static readonly Color LINE_COLOR_DARK = new Color32(64, 64, 64, 255);
        static readonly Color BANNER_COLOR_LIGHT = new Color32(64, 64, 64, 255);
        static readonly Color BANNER_COLOR_DARK = new Color32(0, 0, 0, 255);

        public static GUIStyle miniLabel
        {
            get
            {
                if (_miniLabel != null) return _miniLabel;
                _miniLabel = GUI.skin.FindStyle("miniLabel") ??
                             EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("miniLabel");
                return _miniLabel;
            }
        }

        public static GUIStyle miniButton
        {
            get
            {
                if (_miniButton != null) return _miniButton;
                _miniButton = GUI.skin.FindStyle("miniButton") ??
                              EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("miniButton");
                return _miniButton;
            }
        }

        public static int GetMiniLabelWidth(string label)
        {
            int result;
            if (miniLabelWCache.TryGetValue(label, out result)) return result;

            var w = (int) miniLabel.CalcSize(new GUIContent(label)).x;
            miniLabelWCache.Add(label, w);
            return w;
        }

        public static void Tag(Rect r, string label, Color textColor, Color bgColor, float align, Vector2 labelOffset)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var lbw = GetMiniLabelWidth(label);
                var x = Mathf.Max(0, r.width - lbw)*align;
                var dRect = r;
                dRect.x += x;

                var bgColor0 = GUI.backgroundColor;
                var textColor0 = GUI.color;

                GUI.backgroundColor = bgColor;
                GUI.color = textColor;
                {
                    GUI.Label(dRect, label);
                }
                GUI.color = textColor0;
                GUI.backgroundColor = bgColor0;
            }
        }

        public static void TextureColor(Rect r, Texture tex, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;

            if (GUI.color == c)
            {
                GUI.DrawTexture(r, tex);
                return;
            }

            var color0 = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, tex);
            GUI.color = color0;
        }

        public static void SolidColor(Rect r, Color c)
        {
            TextureColor(r, EditorGUIUtility.whiteTexture, c);
        }

        public static void LabelColor(Rect r, string label, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;

            var style = EditorStyles.label;
            var color0 = style.normal.textColor;
            style.normal.textColor = c;
            GUI.Label(r, label, style);
            style.normal.textColor = color0;
        }

        public static void MiniLabelColor(Rect r, string label, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;

            var style = miniLabel;
            var color0 = style.normal.textColor;
            style.normal.textColor = c;
            GUI.Label(r, label, style);
            style.normal.textColor = color0;
        }


        public static bool Toggle(ref bool value, string label)
        {
            var v = GUILayout.Toggle(value, label);
            if (v != value)
            {
                value = v;
                return true;
            }

            return false;
        }

        public static void DrawLine(float px = 0f, float py = 5f, float thick = 1f)
        {
            var rect = GUILayoutUtility.GetRect(0, Screen.width, py*2 + thick, py*2 + thick);
            rect.x += px;
            rect.y += py;
            rect.width -= 2*px;
            rect.height = thick;

            SolidColor(rect,
                EditorGUIUtility.isProSkin ? LINE_COLOR_DARK : LINE_COLOR_LIGHT
                );
        }

        public static bool DrawBanner(bool expand, string label, float dx = 0, float dw = 0)
        {
            var rect = GUILayoutUtility.GetRect(0, Screen.width, 20f, 20f);
            rect.x += dx;
            rect.width += dw;

            var color = EditorGUIUtility.isProSkin
                ? BANNER_COLOR_DARK
                : BANNER_COLOR_LIGHT;

            var labelStyle = EditorGUIUtility.isProSkin
                ? EditorStyles.boldLabel
                : EditorStyles.whiteBoldLabel;

            SolidColor(rect, color);

            if (Event.current.type == EventType.mouseDown)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    expand = !expand;
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.repaint)
            {
                rect.x += 10f;
                rect.y += 2f;
                GUI.Label(rect, label, labelStyle);

                rect.x += rect.width - 16f;
                rect.width = 16f;

                EditorGUI.Foldout(rect, expand, GUIContent.none);
            }

            return expand;
        }


        public static void Check<T>(Rect r, T target, Action<T> click,
            Action<T> rightClick = null,
            Action<T> altClick = null,
            Action<T> ctrlClick = null,
            Action<T> ctrlAltClick = null) where T : Object
        {
            var e = Event.current;

            if (e.type != EventType.mouseDown || e.shift) return; // Don't process shift (use by system) !
            if (!r.Contains(e.mousePosition)) return;

            if (e.button == 0)
            {
                if (e.control)
                {
                    if (e.alt)
                    {
                        if (ctrlAltClick != null)
                        {
                            e.Use();
                            ctrlAltClick(target);
                            //Debug.Log("Ctrl + Alt + Click");
                        }
                        return;
                    }
                    if (ctrlClick != null)
                    {
                        e.Use();
                        ctrlClick(target);
                        //Debug.Log("Ctrl + Click");
                    }
                    return;
                }

                if (e.alt)
                {
                    if (altClick != null)
                    {
                        e.Use();
                        altClick(target);
                        //Debug.Log("Alt + Click");
                    }
                    return;
                }

                if (click != null)
                {
                    e.Use();
                    click(target);
                    //Debug.Log("Click");
                }
            }
            else
            {
                //right click
                if (rightClick != null)
                {
                    e.Use();
                    rightClick(target);
                    //Debug.Log("Ctrl + Alt + Click");
                }
            }
        }
    }
}