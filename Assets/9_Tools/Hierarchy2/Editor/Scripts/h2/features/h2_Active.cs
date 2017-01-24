using System;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Active : h2_Icon
    {
        public h2_Active() : base("h2_active")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Active;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
			Profiler.BeginSample("h2_Active.Draw");
#endif

	        if (h2_Lazy.isMouseDown)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                h2_GUI.Check(icoRect, go, Toggle, null, InvertSibling, UnSmartToggle);

#if H2_DEV
			Profiler.EndSample();
#endif
                return MaxWidth;
            }

	        if (h2_Lazy.isRepaint)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                var idx = go.activeInHierarchy ? 0 : go.activeSelf ? 2 : 1;
                (setting as h2_ActiveIconSetting).DrawIcon(icoRect, idx, go);

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

            var isMulti = h2_Selection.isMultiple;

            switch (cmd)
            {
                case h2_ActiveIconSetting.CMD_TOGGLE:
                    Toggle(go);
                    break;
                case h2_ActiveIconSetting.CMD_TOGGLE_SIBLING:
                    if (!isMulti) ToggleSiblings(go);
                    break;
                case h2_ActiveIconSetting.CMD_TOGGLE_PARENT:
                    if (!isMulti) ToggleParent(go);
                    break;
                case h2_ActiveIconSetting.CMD_TOGGLE_CHILDREN:
                    if (!isMulti) ToggleChildren(go);
                    break;
                case h2_ActiveIconSetting.CMD_ACTIVE_PREV_SIBLING:
                    if (!isMulti) ActivePrevSibling(go);
                    break;
                case h2_ActiveIconSetting.CMD_ACTIVE_NEXT_SIBLING:
                    if (!isMulti) ActiveNextSibling(go);
                    break;

                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }

            h2_Utils.DelayRepaintHierarchy();
        }

        // ------------------------------ UTILS -----------------------------------

        internal static void SetActive(GameObject go, bool value, string undoName, h2_ParentAction parentAction)
        {
            Undo.RecordObject(go, undoName);
            go.SetActive(value);

            if (parentAction == h2_ParentAction.None) return;
            var pValue = parentAction == h2_ParentAction.Active;

            var p = go.transform.parent;

            while (p != null)
            {
                Undo.RecordObject(p.gameObject, undoName);
                p.gameObject.SetActive(pValue);
                p = p.parent;
            }
        }

        internal static void SetActiveArray(bool v, GameObject[] goList, string undoName, GameObject exclude = null)
        {
            for (var i = 0; i < goList.Length; i++)
            {
                var item = goList[i];
                if (item == exclude) continue;
                SetActive(item, v, undoName, v ? h2_ParentAction.Active : h2_ParentAction.None);
            }
        }

        // ------------------------------- APIS -----------------------------------

        internal void Toggle(GameObject go) // Click
        {
            var v = !go.activeSelf;

            Undo.IncrementCurrentGroup();
            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                var undoName = (v ? "Deactive " : "Active ") + arr.Length + " GameObjects";
                SetActiveArray(v, arr, undoName);
            }
            else
            {
                var undoName = (v ? "Deactive " : "Active ") + go.name;
                SetActive(go, v, undoName, v ? h2_ParentAction.Active : h2_ParentAction.None);
            }
        }

        internal void InvertSibling(GameObject go) // Alt + Click
        {
            var v = go.activeSelf;
            var undoName = "Invert Active";
            var inSelection = h2_Selection.PartOfMuti(go);

            Undo.IncrementCurrentGroup();
            SetActiveArray(v, inSelection ? h2_Selection.gameObjects : h2_Unity.GetSiblings(go).ToArray(), undoName);
            SetActive(go, !v, undoName, inSelection ? h2_ParentAction.None : h2_ParentAction.Active);
        }

        internal void UnSmartToggle(GameObject go) // Ctrl + Click
        {
            var v = !go.activeSelf;

            Undo.IncrementCurrentGroup();
            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                var undoName = (v ? "Deactive " : "Active ") + arr.Length + " GameObjects";
                SetActiveArray(v, arr, undoName);
            }
            else
            {
                var undoName = (v ? "Deactive " : "Active ") + go.name;
                SetActive(go, v, undoName, v ? h2_ParentAction.None : h2_ParentAction.Deactive);
            }
        }

        internal void ToggleParent(GameObject go)
        {
            var v = !go.activeSelf;
            var p = go.transform;

            Undo.IncrementCurrentGroup();
            while (p != null)
            {
                Undo.RecordObject(p.gameObject, "Toggle Active Parent");
                p.gameObject.SetActive(v);
                p = p.parent;
            }
        }

        internal void ToggleSiblings(GameObject go)
        {
            Undo.IncrementCurrentGroup();
            SetActiveArray(!go.activeSelf, h2_Unity.GetSiblings(go).ToArray(), "Toggle Active Siblings");
        }

        internal void ToggleChildren(GameObject go)
        {
            var v = !go.activeSelf;

            Undo.IncrementCurrentGroup();
            Undo.RecordObject(go, "Toggle Active Children");
            go.SetActive(v);

            foreach (Transform t in go.transform)
            {
                Undo.RecordObject(t.gameObject, "Toggle Active Children");
                t.gameObject.SetActive(v);
            }
        }

        internal void ActivePrevSibling(GameObject go)
        {
            var siblings = h2_Unity.GetSiblings(go);
            if (siblings.Count <= 1) return;

            var idx = siblings.IndexOf(go);
            var prev = siblings[(idx - 1 + siblings.Count)%siblings.Count];

            Undo.IncrementCurrentGroup();
            SetActiveArray(false, siblings.ToArray(), "Active Prev Sibling", prev);
            SetActive(prev, true, "Active Prev Sibling", h2_ParentAction.Active);

            Selection.activeGameObject = prev;
        }

        internal void ActiveNextSibling(GameObject go)
        {
            var siblings = h2_Unity.GetSiblings(go);
            if (siblings.Count <= 1) return;

            var idx = siblings.IndexOf(go);
            var next = siblings[(idx + 1 + siblings.Count)%siblings.Count];

            Undo.IncrementCurrentGroup();
            SetActiveArray(false, siblings.ToArray(), "Active Prev Sibling", next);
            SetActive(next, true, "Active Prev Sibling", h2_ParentAction.Active);

            Selection.activeGameObject = next;
        }

        internal enum h2_ParentAction
        {
            None,
            Active,
            Deactive
        }
    }

    [Serializable]
    internal class h2_ActiveIconSetting : h2_IconSetting
    {
        internal const string CMD_TOGGLE = "toggle";
        internal const string CMD_TOGGLE_SIBLING = "toggle_siblings";
        internal const string CMD_TOGGLE_PARENT = "toggle_parent";
        internal const string CMD_TOGGLE_CHILDREN = "toggle_children";
        internal const string CMD_ACTIVE_PREV_SIBLING = "active_prev_sibling";
        internal const string CMD_ACTIVE_NEXT_SIBLING = "active_next_sibling";

        const string TITLE = "ACTIVE";
        static readonly string[] ACTIVE_ICONS = {"eye", "eye_dis", "eye_dis"};
        static readonly string[] ACTIVE_STATES = {"Active", "Inactive", "Self"};

        static readonly string[] ACTIVE_SHORTCUTS =
        {
            "Active", CMD_TOGGLE, "_V",
            "Active Siblings", CMD_TOGGLE_SIBLING, string.Empty,
            "Active Parent", CMD_TOGGLE_PARENT, string.Empty,
            "Active Children", CMD_TOGGLE_CHILDREN, string.Empty,
            "Active Prev Sibling", CMD_ACTIVE_PREV_SIBLING, "_<",
            "Active Next Sibling", CMD_ACTIVE_NEXT_SIBLING, "_>"
        };

        internal override void Reset()
        {
            enableIcon = true;
            enableShortcut = true;
            stateTexture = new h2_StateTexture
            {
                states = GetH2Textures(ACTIVE_ICONS)
            };

            shortcuts = h2_Shortcut.FromStrings(ACTIVE_SHORTCUTS);
            h2_Utils.DelayRepaintHierarchy();

#if H2_DEV
		Debug.Log("RESET ACTIVE");
#endif
        }


        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                stateTexture.DrawStates(enableIcon, ACTIVE_STATES);
                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }

        //	c = Color.white;
        //{

        //public Texture2D GetIcon(GameObject go, out Color c)
        //}
        //	Draw(preview, "ACTIVE");
        //{

        //internal override void Draw(bool preview)
        //}
        //       Draw
        //	if (stateTexture == null) Reset();
        //{

        //internal override void DrawExpandContent()
        //	if (go == null || !IsReady) return;	

        //	var idx = go.activeInHierarchy ? 0 : go.activeSelf ? 2 : 1;
        //	var s = stateTexture.states[idx];
        //	var style = s.GetColorIndex(
        //		EditorGUIUtility.isProSkin, 
        //		stateTexture.playColor && EditorApplication.isPlaying,
        //		stateTexture.selectColor && h2_Selection.Contains(go)
        //	);

        //	c = s.colors[style];
        //	return s.texture;
        //}
    }
}