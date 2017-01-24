using System;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    public class h2_Lock : h2_Icon
    {
        public h2_Lock() : base("h2_lock")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Lock;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Lock.Draw");
#endif

	        if (h2_Lazy.isMouseDown)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                h2_GUI.Check(icoRect, go, ToggleLock, null, null, UnSmartToggle);

#if H2_DEV
			Profiler.EndSample();
#endif
                return MaxWidth;
            }

	        if (h2_Lazy.isRepaint)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                (setting as h2_LockSetting).DrawIcon(icoRect, go);
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
                case h2_LockSetting.CMD_TOGGLE_LOCK:
                {
                    var v = !isLocked(go);
                    SetLock(go, v, "Toggle Lock", v ? h2_ChildrenAction.Set : h2_ChildrenAction.Clear);
                    return;
                }

                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }

            h2_Utils.DelayRepaintHierarchy();
        }

        // ------------------------------ UTILS -----------------------------------

        internal static bool isLocked(GameObject go)
        {
            return (go.hideFlags & HideFlags.NotEditable) > 0;
        }

        internal static void SetLock(GameObject go, bool value, string undoName, h2_ChildrenAction childrenAction)
        {
            var components = go.GetComponents<Component>();
            var h = HideFlags.NotEditable | HideFlags.HideInHierarchy;

            // NOTE : Undo not working for HideFlags
            // LOCK components first but Unlock GameObject first (don't know if this really matters)

            if (value)
            {
                foreach (var c in components)
                {
                    if (c == null) continue;
                    if (c is Transform) continue;
                    c.hideFlags |= h;
                }
                go.hideFlags |= HideFlags.NotEditable;
            }
            else // Unlock : Unlock GameObject first
            {
                go.hideFlags &= ~HideFlags.NotEditable;
                foreach (var c in components)
                {
                    if (c == null) continue;
                    if (c is Transform) continue;
                    c.hideFlags &= ~h;
                }
            }

            if (childrenAction == h2_ChildrenAction.None) return;
            var pValue = childrenAction == h2_ChildrenAction.Set;

            var t = go.transform;
            foreach (Transform c in t)
            {
                if (c == t) continue;
                SetLock(c.gameObject, pValue, undoName, childrenAction);
            }
	        
	        h2_Utils.DelayRepaintInspector();
        }

        internal static void SetLockArray(bool v, GameObject[] goList, string undoName, h2_ChildrenAction action,
            GameObject exclude = null)
        {
            for (var i = 0; i < goList.Length; i++)
            {
                var item = goList[i];
                if (item == exclude) continue;
                SetLock(item, v, undoName, action);
            }
        }

        // ------------------------------- APIS -----------------------------------

        internal void ToggleLock(GameObject go) // Click
        {
            var v = !isLocked(go);

            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                var undoName = (v ? "Unlock " : "Lock ") + arr.Length + " GameObjects";
                SetLockArray(v, arr, undoName, h2_ChildrenAction.None);
            }
            else
            {
                var undoName = (v ? "Unlock " : "Lock ") + go.name;
                SetLock(go, v, undoName, v ? h2_ChildrenAction.Set : h2_ChildrenAction.None);
            }
        }

        internal void UnSmartToggle(GameObject go) // Ctrl + Click
        {
            var v = !isLocked(go);

            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                var undoName = (v ? "Unlock " : "Lock ") + arr.Length + " GameObjects";
                SetLockArray(v, arr, undoName, h2_ChildrenAction.None);
            }
            else
            {
                var undoName = (v ? "Unlock " : "Lock ") + go.name;
                SetLock(go, v, undoName, v ? h2_ChildrenAction.None : h2_ChildrenAction.Clear);
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
    internal class h2_LockSetting : h2_IconSetting
    {
        internal const string CMD_TOGGLE_LOCK = "toggle_lock";

        const string TITLE = "LOCK";
        static readonly string[] ICONS = {"lock", "lock_dis"};
        static readonly string[] STATES = {"Locked", "Unlocked"};

        static readonly string[] SHORTCUTS =
        {
            "Toggle lock", CMD_TOGGLE_LOCK, string.Empty
        };

        internal override void Reset()
        {
            enableIcon = true;
            enableShortcut = true;

            stateTexture = new h2_StateTexture
            {
                states = GetH2Textures(ICONS)
            };

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);
            //h2_Utils.DelayRepaintHierarchy();
#if H2_DEV
		Debug.Log("RESET LOCK");
#endif
        }

        public void DrawIcon(Rect r, GameObject go)
        {
            if (go == null || !isReady) return;

            var s = stateTexture.states[h2_Lock.isLocked(go) ? 0 : 1];
            if (s.texture == null) return;

	        var c = s.GetColor(
		    	   h2_Selection.Contains(go),
		    	   EditorGUIUtility.isProSkin,
		    	   h2_Lazy.isFocus,
		    	   EditorApplication.isPlaying
	        );

            h2_GUI.TextureColor(r, s.texture, c);
        }

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