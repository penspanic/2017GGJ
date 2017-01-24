using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Prefab : h2_Icon
    {
        // ----------------------------- API -----------------------------

        const string tempName = "h2_dummy.prefab";

        // ----------------------------- API -----------------------------

        static Dictionary<int, h2_PrefabInfo> cache;

        public h2_Prefab() : base("h2_prefab")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Prefab;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------
        //static Color[] colors = {
        //	new Color32(0,0, 128, 255),
        //	new Color32(128, 0, 0, 255),
        //	new Color32(0, 128, 0, 255)		
        //};

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Prefab.Draw");
#endif
            if (go == null) return 0;


            var info = Get(go, h2_Selection.Contains(go) ? 0.2f : 1f);
            if (info.type == PrefabType.None) return 0;

	        if (h2_Lazy.isMouseDown)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                h2_GUI.Check(icoRect, go, SelectPrefab);

#if H2_DEV
			Profiler.EndSample();
#endif
                return MaxWidth;
            }

	        if (h2_Lazy.isRepaint)
            {
                var icoRect = h2_Utils.subRectRight(r, 16f);
                var s = setting as h2_PrefabSetting;
                //h2_GUI.SolidColor(icoRect, colors[info.colorIndex]);

                s.DrawIcon(icoRect, info.colorIndex, go);

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
                case h2_PrefabSetting.CMD_APPLY_PREFAB:
                    MultiAction(ApplyPrefab, go);
                    return;
                case h2_PrefabSetting.CMD_BREAK_PREFAB:
                    MultiAction(BreakPrefab, go);
                    return;
                case h2_PrefabSetting.CMD_REVERT_PREFAB:
                    MultiAction(RevertPrefab, go);
                    return;
                case h2_PrefabSetting.CMD_ACTIVE_APPLY_PREFAB:
                    MultiAction(ActiveApplyPrefab, go);
                    return;
            }

            Debug.LogWarning("Unsupported command <" + cmd + ">");
        }

        internal static void MultiAction(Action<GameObject> action, GameObject go)
        {
            if (h2_Selection.PartOfMuti(go))
            {
                var arr = h2_Selection.gameObjects;
                for (var i = 0; i < arr.Length; i++)
                {
                    action(arr[i]);
                }
            }
            else
            {
                action(go);
            }
        }


        //https://issuetracker.unity3d.com/issues/redoing-revert-causes-crash
        internal static void BreakPrefab(GameObject go)
        {
            var go2 = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);

            PrefabUtility.DisconnectPrefabInstance(go2);
            var prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + tempName);
            PrefabUtility.ReplacePrefab(go2, prefab, ReplacePrefabOptions.ConnectToPrefab);
            PrefabUtility.DisconnectPrefabInstance(go2);
            AssetDatabase.DeleteAsset("Assets/" + tempName);

            //temp fix to hide Inspector's dirty looks
            Selection.instanceIDs = new int[] {};
        }

        internal static void SelectPrefab(GameObject go)
        {
            var prefab = PrefabUtility.GetPrefabParent(PrefabUtility.FindRootGameObjectWithSameParentPrefab(go));
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab.GetInstanceID());
        }

        internal static void ApplyPrefab(GameObject go)
        {
            var t = PrefabUtility.GetPrefabType(go);
            var v = t == PrefabType.DisconnectedModelPrefabInstance || t == PrefabType.DisconnectedPrefabInstance;

            if (!v) return;
            var pRoot = PrefabUtility.FindValidUploadPrefabInstanceRoot(go);
            var p = PrefabUtility.GetPrefabParent(pRoot);
            PrefabUtility.ReplacePrefab(pRoot, p, ReplacePrefabOptions.ConnectToPrefab);

            //TODO : mark scene dirty
            //TODO : refresh prefab state
        }

        internal static void RevertPrefab(GameObject go)
	    {
		    if (go == null) return;
		    
            var t = PrefabUtility.GetPrefabType(go);
            var v = t == PrefabType.DisconnectedModelPrefabInstance || t == PrefabType.DisconnectedPrefabInstance;

            if (!v) return;

#if UNITY_4_3
		Undo.RegisterCompleteObjectUndo(go, "Revert to prefab");
#else
            Undo.RegisterFullObjectHierarchyUndo(go, "Revert to prefab");
#endif

	        PrefabUtility.ReconnectToLastPrefab(go);
	        
	        //go might be destroyed after reconnect !
	        if (go != null){
	        	PrefabUtility.RevertPrefabInstance(go);
		        Undo.RegisterCreatedObjectUndo(go, "Reconnect prefab");
	        }
        }

        internal static void ActiveApplyPrefab(GameObject go)
        {
            var t = PrefabUtility.GetPrefabType(go);
            var v = t == PrefabType.DisconnectedModelPrefabInstance || t == PrefabType.DisconnectedPrefabInstance;

            if (!v) return;
            var pRoot = PrefabUtility.FindValidUploadPrefabInstanceRoot(go);

            var a = pRoot.activeSelf;
            pRoot.SetActive(true);
            {
                var p = PrefabUtility.GetPrefabParent(pRoot);
                PrefabUtility.ReplacePrefab(pRoot, p, ReplacePrefabOptions.ConnectToPrefab);
            }
            pRoot.SetActive(a);
        }

        static h2_PrefabInfo Get(GameObject go, float expireTime)
        {
            if (cache == null) cache = new Dictionary<int, h2_PrefabInfo>();

            h2_PrefabInfo result;
            var instID = go.GetInstanceID();
            if (cache.TryGetValue(instID, out result))
            {
                if (Time.realtimeSinceStartup - result.timeStamp < expireTime) return result;
            }
            else
            {
                result = new h2_PrefabInfo();
                cache.Add(instID, result);
            }

            result.Refresh(go);
            return result;
        }
    }

    internal class h2_PrefabInfo
    {
        public int colorIndex;
        public Object prefab;
        public GameObject root;
        public float timeStamp;
        public PrefabType type;


        public void Refresh(GameObject go)
        {
            //Debug.Log("h2Prefab.Refresh : " + go);

            timeStamp = Time.realtimeSinceStartup;
            type = PrefabUtility.GetPrefabType(go);
            if (type == PrefabType.None) return;

            root = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
            prefab = PrefabUtility.GetPrefabParent(root);

            switch (type)
            {
                case PrefabType.PrefabInstance:
                case PrefabType.ModelPrefabInstance:
                    colorIndex = 0;
                    break;

                case PrefabType.MissingPrefabInstance:
                    colorIndex = 1;
                    break;

                case PrefabType.DisconnectedModelPrefabInstance:
                case PrefabType.DisconnectedPrefabInstance:
                    colorIndex = 2;
                    break;
            }
        }
    }


    [Serializable]
    internal class h2_PrefabSetting : h2_IconSetting
    {
        internal const string CMD_BREAK_PREFAB = "break_prefab";
        internal const string CMD_APPLY_PREFAB = "apply_prefab";
        internal const string CMD_REVERT_PREFAB = "revert_prefab";
        internal const string CMD_ACTIVE_APPLY_PREFAB = "active_apply_prefab";

        const string TITLE = "PREFAB";
        static readonly string[] STATES = {"Prefab", "Model", "Missing", "Disconnect"};
	    static readonly string[] ICONS = {"cube", "cube", "cube", "cube"};
	    static readonly Color[][] THEME_COLORS = {
	    	new Color[]{ new Color32(128, 196, 255, 255)},
	    	new Color[]{ new Color32(146, 128, 255, 255) },
	    	new Color[]{ new Color32(214, 71, 71, 255)},
	    	new Color[]{ new Color32(193, 193, 193, 255) }
	    };
	    
        static readonly string[] SHORTCUTS =
        {
            "Break Prefab", CMD_BREAK_PREFAB, "#B",
            "Apply Prefab", CMD_APPLY_PREFAB, "#>",
            "Revert Prefab", CMD_REVERT_PREFAB, "#<",
            "Active & Apply Prefab", CMD_ACTIVE_APPLY_PREFAB, "#A"
        };

        internal override void Reset()
        {
	        enableIcon = true;
            enableShortcut = true;

            stateTexture = new h2_StateTexture
            {
	            states = GetH2Textures(ICONS, THEME_COLORS)
            };

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);
            h2_Utils.DelayRepaintHierarchy();

#if H2_DEV
		Debug.Log("RESET PREFAB");
#endif
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