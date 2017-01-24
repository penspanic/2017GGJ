using System;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Static : h2_Icon
    {
        public h2_Static() : base("h2_static")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Static;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Static.Draw");
#endif

	        if (h2_Lazy.isMouseDown)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                h2_GUI.Check(icoRect, go, Toggle, null, null, UnSmartToggle);

#if H2_DEV
			Profiler.EndSample();
#endif
                return MaxWidth;
            }

	        if (h2_Lazy.isRepaint)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                (setting as h2_StaticSetting).DrawIcon(icoRect, go.isStatic ? 0 : 1, go);
#if H2_DEV
			Profiler.EndSample();
#endif
                return MaxWidth;
            }

#if H2_DEV
		Profiler.EndSample();
#endif
            return MaxWidth;
        }

        // ----------------------------- SHORTCUT HANDLER -----------------------------

        protected override void RunCommand(string cmd)
        {
            var go = Selection.activeGameObject;
            if (go == null) return;

            switch (cmd)
            {
                case h2_StaticSetting.CMD_SHOW_STATIC:
                {
                    setting.enableIcon = !setting.enableIcon;
	                EditorUtility.SetDirty(h2_Setting.current);
	                h2_Utils.DelaySaveAssetDatabase();
	                //AssetDatabase.SaveAssets();
                    return;
                }

                case h2_StaticSetting.CMD_TOGGLE_STATIC:
                {
                    var v = !go.isStatic;
                    SetStatic(go, v, "Toggle Static", v ? h2_ChildrenAction.Set : h2_ChildrenAction.Clear);
                    return;
                }

                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }

            h2_Utils.DelayRepaintHierarchy();
        }

        // ------------------------------ UTILS -----------------------------------

        internal static void SetStatic(GameObject go, bool value, string undoName, h2_ChildrenAction childrenAction)
        {
            if (go.isStatic != value)
            {
                Undo.RecordObject(go, undoName);
                go.isStatic = value;
            }

            if (childrenAction == h2_ChildrenAction.None) return;
            var pValue = childrenAction == h2_ChildrenAction.Set;

            var t = go.transform;
            foreach (Transform c in t)
            {
                if (c == t) continue;
                SetStatic(c.gameObject, pValue, undoName, childrenAction);
            }
        }

        internal static void SetStaticArray(bool v, GameObject[] goList, string undoName, GameObject exclude = null)
        {
            for (var i = 0; i < goList.Length; i++)
            {
                var item = goList[i];
                if (item == exclude) continue;
                SetStatic(item, v, undoName, v ? h2_ChildrenAction.Set : h2_ChildrenAction.None);
            }
        }

        // ------------------------------- APIS -----------------------------------

        internal void Toggle(GameObject go) // Click
        {
            var v = !go.isStatic;

            Undo.IncrementCurrentGroup();
            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                var undoName = (v ? "Clear static " : "Set static ") + arr.Length + " GameObjects";
                SetStaticArray(v, arr, undoName);
            }
            else
            {
                var undoName = (v ? "Clear static " : "Set static ") + go.name;
                SetStatic(go, v, undoName, v ? h2_ChildrenAction.Set : h2_ChildrenAction.None);
            }
        }

        internal void UnSmartToggle(GameObject go) // Ctrl + Click
        {
            var v = !go.isStatic;

            Undo.IncrementCurrentGroup();
            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                var undoName = (v ? "Deactive " : "Active ") + arr.Length + " GameObjects";
                SetStaticArray(v, arr, undoName);
            }
            else
            {
                var undoName = (v ? "Deactive " : "Active ") + go.name;
                SetStatic(go, v, undoName, v ? h2_ChildrenAction.None : h2_ChildrenAction.Clear);
            }
        }

        internal enum h2_ChildrenAction
        {
            None,
            Set,
            Clear
        }
    }

    [Serializable]
    internal class h2_StaticSetting : h2_IconSetting
    {
        internal const string CMD_SHOW_STATIC = "show_static";
        internal const string CMD_TOGGLE_STATIC = "toggle_static";

        const string TITLE = "STATIC";
        static readonly string[] ICONS = {"lightning", "lightning"};
        static readonly string[] STATES = {"Static", "Dynamic"};

        static readonly string[] SHORTCUTS =
        {
            "Show / Hide static icon", CMD_SHOW_STATIC, string.Empty,
            "Toggle static", CMD_TOGGLE_STATIC, string.Empty
        };

        internal override void Reset()
        {
            enableIcon = false;
            enableShortcut = true;

            stateTexture = new h2_StateTexture
            {
                states = GetH2Textures(ICONS)
            };

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);
            h2_Utils.DelayRepaintHierarchy();
#if H2_DEV
		Debug.Log("RESET STATIC");
#endif
        }

        //public void DrawIcon(Rect r, GameObject go)
        //{
        //	if (go == null || !isReady) return;

        //	var s = stateTexture.states[go.isStatic ? 0 : 1];
        //	if (s.texture == null) return;

        //	var c = s.GetColor(
        //		EditorGUIUtility.isProSkin,
        //		stateTexture.playColor && EditorApplication.isPlaying,
        //		stateTexture.selectColor && h2_Selection.Contains(go)
        //	);

        //	h2_GUI.TextureColor(r, s.texture, c);
        //}

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                stateTexture.DrawStates(enableIcon, STATES);
                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}