using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_5_3_OR_NEWER
using UnityParticleSystem = UnityEngine.ParticleSystem;

#else
	using UnityParticleSystem = UnityEngine.ParticleEmitter;
#endif

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    internal class h2_Component : h2_Icon
    {
        internal static int stIdx = -1;

        public h2_Component() : base("h2_component")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Component;
        }

        // ------------------------------ HIERARCHY ICON -----------------------------

        public override float Draw(Rect r, GameObject go)
        {
#if H2_DEV
		Profiler.BeginSample("h2_Component.Draw");
#endif

            if (stIdx == -1)
            {
                stIdx = (setting as h2_ComponentSetting).showTransform ? 0 : 1;
            }

            var info = h2_GOCache.Get(go, h2_Selection.Contains(go) ? 1f : 60f);
            var w = (info.nComponents - stIdx)*16f;
            if (w > MaxWidth) MaxWidth = w;

            var tr = r.x + r.width;
            var drawRect = new Rect(tr - w, r.y, w, r.height);

	        if (h2_Lazy.isMouseDown)
            {
                var mPos = Event.current.mousePosition;

                if (drawRect.Contains(mPos))
                {
                    //Debug.Log(tr + ":" + (tr - mPos.x));
                    var cIdx = Mathf.FloorToInt((tr - mPos.x)/16f) + stIdx;
                    if (cIdx >= info.components.Count || cIdx < 0)
                    {
                        Debug.LogWarning("cIdx is invalid : " + cIdx + ":" + info.components.Count + ":" + go);
                    }
                    else
                    {
                        var cc = info.components[cIdx];
                        if (cc.type.hasEnableProp)
                        {
                            cc.isEnabled = !cc.isEnabled;

                            Undo.IncrementCurrentGroup();

                            if (h2_Selection.PartOfMuti(go))
                            {
                                var arr = h2_Selection.gameObjects;
                                var listCC = new List<Component>();
                                for (var i = 0; i < arr.Length; i++)
                                {
                                    var item = h2_GOCache.Get(arr[i], 1f);
                                    for (var j = 0; j < item.components.Count; j++)
                                    {
                                        var cj = item.components[j];

                                        if ((cj.type == cc.type) && (cj.c != null))
                                        {
                                            cj.isEnabled = cc.isEnabled;
                                            listCC.Add(cj.c);
                                        }
                                    }
                                }

                                for (var k = 0; k < listCC.Count; k++)
                                {
                                    h2_ComponentType.SetEnable(listCC[k], cc.isEnabled);
                                }
                            }
                            else
                            {
                                h2_ComponentType.SetEnable(cc.c, cc.isEnabled);
                            }

                            Event.current.Use();
                            h2_Unity.InspectorWindow.Repaint();
                        }
                    }
                }

#if H2_DEV
			Profiler.EndSample();
#endif
                return w;
            }

	        if (h2_Lazy.isRepaint)
            {
                var icoRect = new Rect(tr - 16f, r.y, 16f, r.height);
                for (var i = stIdx; i < info.nComponents; i++)
                {
                    var cc = info.components[i];
                    var icon = AssetPreview.GetMiniThumbnail(cc.c);
                    if (icon == null) continue;

                    if (cc.isEnabled)
                    {
                        GUI.DrawTexture(icoRect, icon);
                    }
                    else
                    {
                        h2_GUI.TextureColor(icoRect, icon, Color.black);
                    }

                    icoRect.x -= 16f;
                }

#if H2_DEV
			Profiler.EndSample();
#endif
                return w;
            }

#if H2_DEV
		Profiler.EndSample();
#endif
            return w;
        }

        // ----------------------------- SHORTCUT HANDLER -----------------------------

        protected override void RunCommand(string cmd)
        {
            switch (cmd)
            {
                case h2_ComponentSetting.CMD_SHOW_COMPONENT:
                {
                    setting.enableIcon = !setting.enableIcon;

	                EditorUtility.SetDirty(h2_Setting.current);
	                h2_Utils.DelaySaveAssetDatabase();
	                //AssetDatabase.SaveAssets();
                    return;
                }

                case h2_ComponentSetting.CMD_SHOW_CUSTOMIZED:
                {
                    Debug.LogWarning("Not yet implemented");
                    return;
                }

                case h2_ComponentSetting.CMD_RESET_LC_TRANSFORM:
                {
                    ResetSelectedTransforms(true, true, true, "Reset Transform");
                    return;
                }
                case h2_ComponentSetting.CMD_RESET_LC_POSITION:
                {
                    ResetSelectedTransforms(true, false, false, "Reset Position");
                    return;
                }
                case h2_ComponentSetting.CMD_RESET_LC_ROTATION:
                {
                    ResetSelectedTransforms(false, true, false, "Reset Rotation");
                    return;
                }
                case h2_ComponentSetting.CMD_RESET_LC_SCALE:
                {
                    ResetSelectedTransforms(false, false, true, "Reset Scale");
                    return;
                }

                case h2_ComponentSetting.CMD_ALT_RESET_LC_TRANSFORM:
                {
                    AltResetSelectedTransforms(true, true, true, "Alt Reset Transform");
                    return;
                }
                case h2_ComponentSetting.CMD_ALT_RESET_LC_POSITION:
                {
                    AltResetSelectedTransforms(true, false, false, "Alt Reset Position");
                    return;
                }

                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }

            h2_Utils.DelayRepaintHierarchy();
        }


        // ------------------------------ UTILS -----------------------------------

        public static void ResetSelectedTransforms(bool position, bool rotation, bool scale, string undoName)
        {
            var list = h2_Selection.gameObjects;
            if (list == null || list.Length == 0) return;

            Undo.IncrementCurrentGroup();

            for (var i = 0; i < list.Length; i++)
            {
                var t = list[i].transform;

                Undo.RecordObject(t, undoName);
                if (position) t.localPosition = Vector3.zero;
                if (rotation) t.localRotation = Quaternion.identity;
                if (scale) t.localScale = Vector3.one;
            }
        }


        public static void AltResetSelectedTransforms(bool position, bool rotation, bool scale, string undoName)
        {
            var list = h2_Selection.gameObjects;
            if (list == null || list.Length == 0) return;

            var tempT = new GameObject().transform;

            Undo.IncrementCurrentGroup();

            for (var i = 0; i < list.Length; i++)
            {
                var t = list[i].transform;
                if (t.childCount == 0) continue;

                tempT.parent = t.parent;
                tempT.localPosition = Vector3.zero;
                tempT.localRotation = Quaternion.identity;
                tempT.localScale = Vector3.one;

                MoveChildren(t, tempT, undoName);

                Undo.RecordObject(t, undoName);
                if (position) t.localPosition = Vector3.zero;
                if (rotation) t.localRotation = Quaternion.identity;
                if (scale) t.localScale = Vector3.one;

                MoveChildren(tempT, t, null);
            }

            Object.DestroyImmediate(tempT.gameObject);
        }

        static void MoveChildren(Transform from, Transform to, string undoName)
        {
            if (to.childCount > 0)
            {
                Debug.LogWarning("Something wrong, move target should not contains any children !");
            }

            for (var i = from.childCount - 1; i >= 0; i--)
            {
                var c = from.GetChild(i);

                if (!string.IsNullOrEmpty(undoName)) Undo.RecordObject(c, undoName);
                from.GetChild(i).parent = to;
            }
        }
    }

    internal class h2_ComponentType
    {
        // --------------------------- CACHE -----------------------------

        static Dictionary<Type, h2_ComponentType> typeCache;
        public bool hasEnableProp;
        public bool isMonoBehaviour;
        public Type type;
        //public h2_ComponentCustom customIcon;

        public h2_ComponentType(Component c)
        {
            type = c.GetType();

            isMonoBehaviour = c is MonoBehaviour;
            hasEnableProp = isMonoBehaviour
                            || c is Renderer
                            || c is Behaviour
                            || c is Collider
                            || c is Cloth
                            || c is LODGroup
                            || c is UnityParticleSystem;
        }

        internal static h2_ComponentType Get(Component c)
        {
            if (typeCache == null) typeCache = new Dictionary<Type, h2_ComponentType>();

            var cType = c.GetType();

            h2_ComponentType result;
            if (typeCache.TryGetValue(cType, out result)) return result;

            result = new h2_ComponentType(c);
            typeCache.Add(cType, result);

            return result;
        }

        public static void SetEnable(Component c, bool v)
        {
            if (c == null) return;

            Undo.RecordObject(c, v ? "Enable Component" : "Disalbe Component");

            if (c is Behaviour)
            {
                (c as Behaviour).enabled = v;
                return;
            }

            if (c is Renderer)
            {
                (c as Renderer).enabled = v;
                return;
            }

            if (c is Collider)
            {
                (c as Collider).enabled = v;
                return;
            }

            if (c is Cloth)
            {
                (c as Cloth).enabled = v;
                return;
            }

            if (c is LODGroup)
            {
                (c as LODGroup).enabled = v;
                return;
            }

            if (c is UnityParticleSystem)
            {
#if UNITY_5_3_OR_NEWER
                // IMPORTANT NOTE : CAN NOT SET .enable DIRECTLY BUT NEED TO STORE AS A LOCAL VARIABLE
                // WEIRD ? Ask Unity why, I don't know !
                var emission = (c as UnityParticleSystem).emission;
                emission.enabled = v;
#else
				(c as UnityParticleSystem).enabled = v;
#endif
                return;
            }

            Debug.LogWarning("Unsupport component <" + c + ">");
        }

        public static bool GetEnabled(Component c)
        {
            if (c == null) return false;

            if (c is Behaviour) return (c as Behaviour).enabled;
            if (c is Renderer) return (c as Renderer).enabled;
            if (c is Collider) return (c as Collider).enabled;
            if (c is Cloth) return (c as Cloth).enabled;
            if (c is LODGroup) return (c as LODGroup).enabled;
            if (c is UnityParticleSystem)
            {
#if UNITY_5_3_OR_NEWER
                return (c as UnityParticleSystem).emission.enabled;
#else
			return (c as UnityParticleSystem).enabled;
#endif
            }

            Debug.LogWarning("Unsupport component <" + c + ">");
            return true;
        }
    }

    internal class h2_GOCache
    {
        // --------------------------- CACHE -----------------------------

        static Dictionary<int, h2_GOCache> goCache;
        public float cacheTime;
        public List<h2_ComponentCache> components;
        public float enableTime;

        public int nComponents;
        public int nCustomized;

        public void RefreshEnable()
        {
            enableTime = Time.realtimeSinceStartup;
            for (var i = 0; i < components.Count; i++)
            {
                var cc = components[i];
                if (cc.c == null) continue;

                if (!cc.type.hasEnableProp) continue;

                try
                {
                    cc.isEnabled = h2_ComponentType.GetEnabled(cc.c);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        public void Refresh(GameObject go)
        {
            cacheTime = Time.realtimeSinceStartup;
            enableTime = cacheTime;

            nComponents = 0;
            nCustomized = 0;

            var dict = new Dictionary<Component, h2_ComponentCache>();
            if (components != null)
            {
                for (var i = 0; i < components.Count; i++)
                {
                    var cc = components[i];
                    if (cc.c != null)
                    {
                        dict.Add(cc.c, cc);
                    }
                }

                components.Clear();
            }
            else
            {
                components = new List<h2_ComponentCache>();
            }

            var cList = go.GetComponents<Component>();
            for (var i = 0; i < cList.Length; i++)
            {
                var c = cList[i];
                if (c == null) continue;

                h2_ComponentCache cc;

                if (!dict.TryGetValue(c, out cc))
                {
                    cc = new h2_ComponentCache
                    {
                        c = c,
                        type = h2_ComponentType.Get(c)
                    };
                }

                cc.isEnabled = cc.type.hasEnableProp ? h2_ComponentType.GetEnabled(cc.c) : true;
                nComponents++;
                components.Add(cc);
            }
        }

        internal static h2_GOCache Get(GameObject go, float expire)
        {
            if (goCache == null) goCache = new Dictionary<int, h2_GOCache>();

            h2_GOCache result;
            var instID = go.GetInstanceID();
            if (goCache.TryGetValue(instID, out result))
            {
                var realTime = Time.realtimeSinceStartup;
                if (realTime - result.cacheTime < expire)
                {
                    //Refresh enable property every 0.5 second : useful in undo case
                    if (realTime - result.enableTime > 0.5f)
                    {
                        result.RefreshEnable();
                    }
                    return result;
                }
            }
            else
            {
                result = new h2_GOCache();
                goCache.Add(instID, result);
            }
            result.Refresh(go);
            return result;
        }
    }

    internal class h2_ComponentCache
    {
        public Component c;
        public bool isEnabled;
        public h2_ComponentType type;
    }

    //[Serializable] internal class h2_ComponentCustom {
    //	public string className;
    //	public Texture2D tex;
    //	public Color color;
    //}

    [Serializable]
    internal class h2_ComponentSetting : h2_FeatureSetting
    {
        internal const string CMD_SHOW_COMPONENT = "show_component";
        internal const string CMD_SHOW_CUSTOMIZED = "show_customize";

        internal const string CMD_RESET_LC_TRANSFORM = "reset_lc_transform";
        internal const string CMD_RESET_LC_POSITION = "reset_lc_position";
        internal const string CMD_RESET_LC_ROTATION = "reset_lc_rotation";
        internal const string CMD_RESET_LC_SCALE = "reset_lc_scale";

        internal const string CMD_ALT_RESET_LC_TRANSFORM = "alt_reset_lc_transform";
        internal const string CMD_ALT_RESET_LC_POSITION = "alt_reset_lc_position";


        const string TITLE = "COMPONENT";

        static readonly string[] SHORTCUTS =
        {
            "Show Components", CMD_SHOW_COMPONENT, "#%&C",
            "Show Customized Icons", CMD_SHOW_CUSTOMIZED, string.Empty,
            "Reset Local Transform", CMD_RESET_LC_TRANSFORM, "#T",
            "Reset Local Position", CMD_RESET_LC_TRANSFORM, "#P",
            "Reset Local Rotation", CMD_RESET_LC_TRANSFORM, "#R",
            "Reset Local Scale", CMD_RESET_LC_TRANSFORM, "#S",
            "Reset Local Transform", CMD_ALT_RESET_LC_TRANSFORM, "#&T",
            "Reset Local Position", CMD_ALT_RESET_LC_TRANSFORM, "#&P"
        };

        //public h2_ComponentCustom[] customizedIcons;

        public bool showTransform;

        internal override void Reset()
        {
            enableIcon = false;
            enableShortcut = true;

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);
            h2_Utils.DelayRepaintHierarchy();

#if H2_DEV
		Debug.Log("RESET COMPONENT");
#endif
        }

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                var v = GUILayout.Toggle(showTransform, "Show Transform");
                if (v != showTransform)
                {
                    showTransform = v;
                    h2_Component.stIdx = -1;
                }

                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}