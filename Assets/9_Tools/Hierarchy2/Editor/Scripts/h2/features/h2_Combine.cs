#define CACHE_INT

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Combine : h2_Icon
    {
        static Dictionary<int, string> intMap;

        // ------------------------------ HIERARCHY ICON -----------------------------

#if CACHE_INT
        static readonly Dictionary<int, string> intCache = new Dictionary<int, string>();
#endif

        public h2_Combine() : base("h2_Combine", 26f)
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Combine;
        }

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
			Profiler.BeginSample("h2_Combine.Draw");
#endif

            if (go == null || go.transform.childCount == 0)
            {
#if H2_DEV
			Profiler.EndSample();
#endif
                return 0;
            }

	        if (h2_Lazy.isMouseDown)
            {
                var icoRect = h2_Utils.subRectRight(r, MaxWidth);
                h2_GUI.Check(icoRect, go, CombineGO);
#if H2_DEV
			Profiler.EndSample();
#endif
                return MaxWidth;
            }

	        if (h2_Lazy.isRepaint)
            {
                var t = go.transform;
                var n = t.childCount;

#if CACHE_INT
                string lb;
                if (!intCache.TryGetValue(n, out lb))
                {
                    lb = n.ToString();
                    intCache.Add(n, lb);
                }
                ;
#else
			var lb = n.ToString();
#endif

                var v = isCombined(t);
                var icoRect2 = h2_Utils.subRectRight(r, MaxWidth);

                if (v)
                {
                    var tex = h2_GUI.miniButton.normal.background;
                    //GUI.DrawTexture(new Rect(icoRect2.x, icoRect2.y + 2, tex.width, tex.height), tex);
                    icoRect2.height = tex.height;
                    GUI.Button(icoRect2, lb, h2_GUI.miniButton);
                }
                else 
                {
                    var w = h2_GUI.GetMiniLabelWidth(lb);
                    icoRect2.x += icoRect2.width - w;
                    icoRect2.width = w;
                    GUI.Label(icoRect2, lb, h2_GUI.miniLabel);
                }
            }

#if H2_DEV
		Profiler.EndSample();
#endif
            return MaxWidth;
        }

        protected override void RunCommand(string cmd)
        {
            if (Selection.activeGameObject == null || h2_Selection.isMultiple) return;

            switch (cmd)
            {
                case "combine":
                    CombineGO(Selection.activeGameObject);
                    break;
            }
        }

        //override protected h2_FeatureSetting setting {
        //	get { return h2_Setting.current.Combine; }
        //}

        // ------------------------------- APIS -----------------------------------

        bool isCombined(Transform t)
        {
            //Debug.Log(t.GetChild(0).hideFlags  & HideFlags.HideInHierarchy);
            return (t.GetChild(0).hideFlags & HideFlags.HideInHierarchy) == HideFlags.HideInHierarchy;
        }

        public void CombineGO(GameObject go)
        {
            if (go == null) return;
            var t = go.transform;
            if (t.childCount == 0) return;

            //Hideflags can not be Undo - will think of a workaround when have time

            var v = !isCombined(t);
            for (var i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i).gameObject;
                if (v)
                {
                    c.hideFlags |= HideFlags.HideInHierarchy;
                }
                else
                {
                    c.hideFlags &= ~HideFlags.HideInHierarchy;
                }
            }

            h2_Unity.SetExpand(go, false);
            h2_Unity.SetExpand(go, true);
        }
    }

    [Serializable]
    internal class h2_CombineSetting : h2_FeatureSetting
    {
        const string TITLE = "CHILDREN COUNT";

        static readonly string[] COMBINE_SHORTCUTS =
        {
            "Combine", "combine", string.Empty
        };

        internal override void Reset()
        {
            enableIcon = true;
            enableShortcut = true;
            shortcuts = h2_Shortcut.FromStrings(COMBINE_SHORTCUTS);

#if H2_DEV
		Debug.Log("RESET COMBINE");
#endif
        }

        public void DrawInspector()
        {
            DrawBanner(TITLE, true, true);
        }
    }
}