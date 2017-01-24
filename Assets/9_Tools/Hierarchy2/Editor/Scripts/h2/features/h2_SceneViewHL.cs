using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    public class h2_SceneViewHL : h2_Icon
    {
        // -------------------------------- API ------------------------------


        private static bool _isFocus;
        private static bool _isEnabled;
        private static GameObject _hoveredInstance;

        internal static Color CYAN_A50 = new Color32(0, 255, 255, 128);
        internal static Color RED_A50 = new Color32(255, 0, 0, 128);
        internal static Color GREEN_A50 = new Color32(0, 255, 0, 128);
        internal static Color BLUE_A50 = new Color32(0, 0, 255, 128);

        private static GameObject cacheTarget;
        private static Bounds? selfBound;
        private static Bounds? fullBound;


        public h2_SceneViewHL() : base("h2_sceneview_hl", 0f)
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.SceneViewHL;
        }

        public override float Draw(Rect r, GameObject go)
        {
	        if (!h2_Lazy.isFocus || go == null) return 0;
            if (!_isEnabled) StartMonitor();

            //if (Event.current.type == EventType.mouseMove)
            {
                var e = Event.current;
                var mPos = e.mousePosition;
                var rr = new Rect(0, r.y, r.x + r.width, r.height);
                if (rr.Contains(mPos))
                {
                    if (_hoveredInstance != go)
                    {
                        _hoveredInstance = go;
                        SceneView.RepaintAll();

                        if (e.alt)
                        {
                            h2_Unity.FocusInScene(go);
                        }
                    }
                }
            }

	        if (h2_Lazy.isRepaint && go == _hoveredInstance)
            {
                var rr = new Rect(0, r.y, r.x + r.width, r.height);
                h2_GUI.SolidColor(rr, new Color32(0, 128, 0, 64));
            }

            return 0;
        }

        protected override void RunCommand(string cmd)
        {
            var go = Selection.activeGameObject;
            if (go == null) return;

            switch (cmd)
            {
                case h2_SceneViewHLSetting.CMD_ENABLE:
                {
                    setting.enableIcon = !setting.enableIcon;
	                EditorUtility.SetDirty(h2_Setting.current);
	                h2_Utils.DelaySaveAssetDatabase();
	                //AssetDatabase.SaveAssets();
                    return;
                }

                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }

            h2_Utils.DelayRepaintHierarchy();
        }

        private static void OnSceneGUIDelegate(SceneView sceneView)
        {
            if (!h2_Setting.current.SceneViewHL.enableIcon)
            {
                StopMonitor();
                return;
            }

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                case EventType.DragExited:
                    sceneView.Repaint();
                    break;
            }

            if (Event.current.type == EventType.repaint)
            {
                var drawGOs = new HashSet<GameObject>();

                var handleColor = Handles.color;

                Handles.color = h2_Setting.current.SceneViewHL.dragColor;
                foreach (var objectReference in DragAndDrop.objectReferences)
                {
                    var gameObject = objectReference as GameObject;

                    if (gameObject && gameObject.activeInHierarchy)
                    {
                        DrawObjectBounds(gameObject);
                        drawGOs.Add(gameObject);
                    }
                }

                Handles.color = h2_Setting.current.SceneViewHL.hoverColor;
                if (_hoveredInstance != null && !drawGOs.Contains(_hoveredInstance))
                {
                    DrawObjectBounds(_hoveredInstance);
                }

                Handles.color = handleColor;
            }
        }

        private static void DrawObjectBounds(GameObject go)
        {
            if (cacheTarget != go)
            {
                cacheTarget = go;

                if (cacheTarget != null)
                {
                    // my bound
                    var renderers = go.GetComponents<Renderer>();
                    if (renderers.Length > 0)
                    {
                        var b = renderers[0].bounds;
                        for (var i = 1; i < renderers.Length; i++)
                        {
                            b.Encapsulate(renderers[i].bounds);
                        }
                        selfBound = b;
                    }
                    else
                    {
                        selfBound = null;
                    }

                    //full bound
                    renderers = go.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        var b = renderers[0].bounds;
                        for (var i = 1; i < renderers.Length; i++)
                        {
                            b.Encapsulate(renderers[i].bounds);
                        }
                        fullBound = b;
                    }
                    else
                    {
                        fullBound = null;
                    }
                }
                else
                {
                    selfBound = null;
                }
            }


            DrawBox(selfBound, null, Color.cyan);
            DrawBox(fullBound, go.transform, Color.yellow);
        }

        private static void DrawBox(Bounds? bound, Transform t, Color c)
        {
            var oColor = Handles.color;
            Handles.color = CYAN_A50;

            if (bound != null)
            {
                var b = bound.Value;
                var v0 = b.min;
                var v7 = b.max;

                var v1 = new Vector3(v0.x, v0.y, v7.z);
                var v2 = new Vector3(v0.x, v7.y, v7.z);
                var v3 = new Vector3(v0.x, v7.y, v0.z);

                var v4 = new Vector3(v7.x, v0.y, v7.z);
                var v5 = new Vector3(v7.x, v0.y, v0.z);
                var v6 = new Vector3(v7.x, v7.y, v0.z);


                Handles.DrawPolyLine(
                    v0, v1, v2, v3, v0,
                    v5, v6, v7, v4, v5
                    );

                Handles.DrawLine(v1, v4);
                Handles.DrawLine(v2, v7);
                Handles.DrawLine(v3, v6);
            }

            if (t != null)
            {
                //Handles.ArrowCap(0, t.position, t.rotation, 1f);
                if (bound != null)
                {
                    Handles.DrawLine(t.position, bound.Value.center);
                }

                var sz = HandleUtility.GetHandleSize(t.position);

                Handles.color = GREEN_A50;
                Handles.ArrowCap(0,
                    t.position,
                    Quaternion.LookRotation(t.up),
                    sz);
                Handles.color = BLUE_A50;
                Handles.ArrowCap(0,
                    t.position,
                    Quaternion.LookRotation(t.forward),
                    sz);
                Handles.color = RED_A50;
                Handles.ArrowCap(0,
                    t.position,
                    Quaternion.LookRotation(t.right),
                    sz);
            }

            Handles.color = oColor;
        }

        //internal static void Draw(int instID, Rect r) {
        //        //Debug.Log(Event.current + ":" + WindowX.Hierarchy.wantsMouseMove);

        //	if (Event.current.type == EventType.Layout) {
        //		_isFocus = EditorWindow.focusedWindow == WindowX.Hierarchy;
        //		return;
        //	}

        //	if (_hoveredInstance == instID) { //Draw an indicator
        //		GUI.DrawTexture(r.l(0), Color.green.xAlpha(0.1f).xGetTexture2D());
        //	}

        //	if (!_isFocus) return;

        //	var current = Event.current;
        //	switch (Event.current.type) {
        //	case EventType.MouseMove:
        //		var mouse   = current.mousePosition;
        //		var inside = r.yMin < mouse.y && mouse.y < r.yMax;
        //		if (inside) {
        //			_hoveredInstance = instID;
        //			WindowX.Hierarchy.Repaint();

        //		} else if (_hoveredInstance == instID) {
        //			_hoveredInstance = 0;
        //		}

        //		var sv = SceneView.lastActiveSceneView;
        //		if (sv) sv.Repaint();

        //		break;

        //	case EventType.MouseDrag:
        //	case EventType.DragUpdated:
        //	case EventType.DragPerform:
        //	case EventType.DragExited:
        //		sv = SceneView.lastActiveSceneView;
        //		if (sv) sv.Repaint();
        //		break;
        //	}
        //        //Debug.Log("Focus " + _hoveredInstance + ":" + Event.current.type + ":" + r);
        //}

        public static void StartMonitor()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUIDelegate;
            SceneView.onSceneGUIDelegate += OnSceneGUIDelegate;
            _isEnabled = true;
            h2_Unity.HierarchyWindow.wantsMouseMove = _isEnabled;
        }

        public static void StopMonitor()
	    {
		    //Debug.Log("Stop Monitor");
            SceneView.onSceneGUIDelegate -= OnSceneGUIDelegate;
            _isEnabled = false;
            h2_Unity.HierarchyWindow.wantsMouseMove = _isEnabled;
        }
    }

    [Serializable]
    internal class h2_SceneViewHLSetting : h2_FeatureSetting
    {
        internal const string CMD_ENABLE = "enable";

        const string TITLE = "SCENEVIEW HIGHLIGHT";

        static readonly string[] SHORTCUTS =
        {
            "Toggle SceneView Highlight", CMD_ENABLE, "#%&S"
        };

        public Color dragColor;


        //------------------------ INSTANCE -------------------------

        public Color hoverColor;

        internal override void Reset()
        {
            enableIcon = false;
            enableShortcut = true;

            hoverColor = new Color(0f, 1f, 1f, 0.75f);
            dragColor = new Color(1f, 1f, 0, 0.75f);

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);
            h2_Utils.DelayRepaintHierarchy();

#if H2_DEV
		Debug.Log("RESET SCENEVIEW HIGHLIGHT");
#endif
        }

        internal void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                var r = GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);
                GUILayout.BeginHorizontal();
                {
                    r.width /= 2f;
                    h2_Color.DrawColorPicker(r, ref hoverColor);

                    r.x += r.width;
                    h2_Color.DrawColorPicker(r, ref dragColor);
                }
                GUILayout.EndHorizontal();

                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}