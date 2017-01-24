using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    public class h2_Script : h2_Icon
    {
        // ----------------------------------  STATIC UTILS --------------------------------

        static Dictionary<GameObject, h2_ScriptInfo> stateMap;
        static Dictionary<Type, bool> typeMap;

        public h2_Script() : base("h2_script", 4f)
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Script;
        }

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
			Profiler.BeginSample("h2_Script.Draw");
#endif

	        if (go == null || !h2_Lazy.isRepaint)
            {
#if H2_DEV
			Profiler.EndSample();
#endif

                return 0;
            }

            var state = GetState(go, h2_Selection.Contains(go) ? 0.1f : 60f);
                //slow refresh for gameObjects that are not selected
            if (state == h2_ScriptState.None)
            {
#if H2_DEV
			Profiler.EndSample();
#endif
                return 0;
            }

            var s = (h2_ScriptSetting) setting;
            var icoRect = h2_Utils.subRectRight(r, MaxWidth);
            h2_GUI.SolidColor(icoRect, s.stateColors[(int) state - 1]);

#if H2_DEV
		Profiler.EndSample();
#endif
            return MaxWidth;
        }

        // ----------------------------------  OVERRIDE --------------------------------

        protected override void RunCommand(string cmd)
        {
            switch (cmd)
            {
                case h2_ScriptSetting.CMD_FIND_MISSING:
                {
                    SelectMissingInScene();
                }
                    return;

                case h2_ScriptSetting.CMD_FIND_MISSING_CHILDREN:
                {
                    SelectMissingInChildren(Selection.activeGameObject);
                    return;
                }
            }

            Debug.LogWarning("Unsupported command <" + cmd + ">");
        }

        static h2_ScriptState GetState(GameObject go, float expire)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Script.GetState");
#endif

            if (stateMap == null)
            {
                stateMap = new Dictionary<GameObject, h2_ScriptInfo>();
            }

            h2_ScriptInfo info;
            if (stateMap.TryGetValue(go, out info))
            {
                var dTime = Time.realtimeSinceStartup - info.time;
                if (dTime < expire)
                {
#if H2_DEV
				Profiler.EndSample();
#endif

                    return info.state;
                }
                stateMap.Remove(go);

                //Debug.Log("Force refresh : " + go + ":" + expire + ":" + dTime);
            }

            //scan for missing reference
            info = new h2_ScriptInfo {state = h2_ScriptState.None};

            var components = go.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                var c = components[i];

                //Debug.Log(i + ":" + c);
                if (c == null)
                {
                    info.state = h2_ScriptState.Missing;
                    //Debug.Log("Missing : " + i);
                    break;
                }

                if (info.state != h2_ScriptState.Script)
                {
                    if (c is MonoBehaviour && isValidScript(c))
                    {
                        info.state = h2_ScriptState.Script;
                        //break;
                    }
                }
            }

            stateMap.Add(go, info);

#if H2_DEV
		Profiler.EndSample();
#endif
            return info.state;
        }

        static bool isValidScript(Component b)
        {
            if (typeMap == null) typeMap = new Dictionary<Type, bool>();

            var typeT = b.GetType();
            var result = false;

            if (typeMap.TryGetValue(typeT, out result)) return result;

            var mono = MonoScript.FromMonoBehaviour((MonoBehaviour) b);
            var path = AssetDatabase.GetAssetPath(mono);

#if H2_DEV
        Debug.Log(typeT + " --> " + path);
#endif

            if (!path.StartsWith("Assets/"))
            {
                typeMap.Add(typeT, false);
                return false;
            }

            var exPaths = h2_Setting.current.Script.excludePaths;
            for (var i = 0; i < exPaths.Length; i++)
            {
                if (path.StartsWith(exPaths[i]))
                {
                    typeMap.Add(typeT, false);
                    return false;
                }
            }

            typeMap.Add(typeT, true);
            return true;
        }

        static List<int> AppendMissing(GameObject go, List<int> result)
        {
            if (result == null) result = new List<int>();
            if (go == null) return result;

            if (GetState(go, 0f) == h2_ScriptState.Missing)
            {
                result.Add(go.GetInstanceID());
            }

            var t = go.transform;
            if (t.childCount > 0)
            {
                for (var i = 0; i < t.childCount; i++)
                {
                    AppendMissing(t.GetChild(i).gameObject, result);
                }
            }

            return result;
        }

        static void SelectMissingInChildren(GameObject go)
        {
	        if (go == null) return;
	        var arr = AppendMissing(go, null).ToArray();
	        
	        if (arr.Length > 0){
	        	Selection.instanceIDs = arr;	
	        } else {
	        	Debug.Log("No missing component found in children of <" + go.name + ">");	
	        }
        }

        static void SelectMissingInScene()
        {
            var root = h2_Unity.GetRootGOs();
            var list = new List<int>();

            for (var i = 0; i < root.Length; i++)
            {
                AppendMissing(root[i], list);
            }
	        
	        if (list.Count > 0){
	        	Selection.instanceIDs = list.ToArray();	
	        } else {
	        	Debug.Log("No missing component found !");	
	        }
        }

        internal enum h2_ScriptState
        {
            None,
            Script,
            Missing
        }

        internal class h2_ScriptInfo
        {
            public h2_ScriptState state;
            public float time;

            public h2_ScriptInfo()
            {
                time = Time.realtimeSinceStartup;
            }
        }
    }

    [Serializable]
    internal class h2_ScriptSetting : h2_IconSetting
    {
        internal const string CMD_FIND_MISSING = "find_missing_script";
        internal const string CMD_FIND_MISSING_CHILDREN = "find_missing_script_in_children";

        const string TITLE = "SCRIPT INDICATOR";

        static readonly GUIContent[] LABELS =
        {
            new GUIContent("Script"),
            new GUIContent("Missing")
        };

        static readonly string[] SHORTCUTS =
        {
	        "Find Missing", CMD_FIND_MISSING, "#M",
	        "Find Missing in Children", CMD_FIND_MISSING_CHILDREN, "#&M"
        };

        //public string[] excludeScriptNames;
        public string[] excludePaths;

        public Color[] stateColors;

        public override bool isReady
        {
            get { return stateColors != null && stateColors.Length > 0; }
        }

        internal override void Reset()
        {
            //Debug.Log("RESET ! " + isReady);
            enableIcon = true;
            enableShortcut = true;

            stateColors = new Color[]
            {
                new Color32(0, 128, 0, 255),
                new Color32(128, 0, 0, 255)
            };

            excludePaths = new[]
            {
                "Assets/Plugins",
                "Assets/Standard Assets/",
                "Assets/Pro Standard Assets/"
            };

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);

#if H2_DEV
		Debug.Log("RESET SCRIPT");
#endif
        }

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                //GUILayout.Space(8f);
                GUILayout.BeginHorizontal();
                {
                    for (var i = 0; i < stateColors.Length; i++)
                    {
                        GUILayout.Label(LABELS[i]);
                        var r = GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);
                        h2_Color.DrawColorPicker(r, ref stateColors[i]);
                        if (i < stateColors.Length - 1) GUILayout.Space(24f);
                    }
                }
                GUILayout.EndHorizontal();

                //GUILayout.Space(8f);
                GUILayout.BeginHorizontal();
                {
                }
                GUILayout.EndHorizontal();

                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}