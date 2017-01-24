using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.h2
{
    public class h2_Common : h2_Icon
    {
        public h2_Common() : base("h2_common", 0f)
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.Common;
        }

        protected override void RunCommand(string cmd)
        {
	        var go = Selection.activeGameObject;
	        
            switch (cmd)
            {
                case h2_CommonSetting.CMD_TOGGLE_ICON:
                {
                    setting.enableIcon = !setting.enableIcon;
                    return;
                }

                case h2_CommonSetting.CMD_OPEN_SETTING:
                {
                    Selection.activeObject = h2_Setting.current;
                    var w = h2_Unity.InspectorWindow;
                    if (w != null) w.Focus();
                    return;
                }

                case h2_CommonSetting.CMD_COPY_NAME:
                {
                    if (go != null) h2_Copy.CopyName(go);
                    return;
                }

                case h2_CommonSetting.CMD_COPY_HIERARCHY_NAME:
                {
                    if (go != null) h2_Copy.CopyHierarchyName(go);
                    return;
                }


                case h2_CommonSetting.CMD_CAMERA_LOOKTHROUGH:
                {
                    if (go != null) h2_Camera.LookThrough(go.GetComponent<Camera>());
                    return;
                }

                case h2_CommonSetting.CMD_CAPTURE_SCENEVIEW:
                {
                    if (go != null) h2_Camera.CaptureSceneView(go.GetComponent<Camera>());
                    return;
                }

                case h2_CommonSetting.CMD_GOTO_ROOT:
                {
                    if (go != null) h2_Goto.GotoRoot(go);
                    return;
                }

                case h2_CommonSetting.CMD_GOTO_PARENT:
                {
                    if (go != null) h2_Goto.GotoParent(go);
                    return;
                }

                case h2_CommonSetting.CMD_GOTO_SIBLING:
                {
                    if (go != null) h2_Goto.GotoSibling(go);
                    return;
                }

                case h2_CommonSetting.CMD_GOTO_CHILD:
                {
                    if (go != null) h2_Goto.GotoChild(go);
                    return;
                }

                case h2_CommonSetting.CMD_SELECT_SIBLING:
                {
                    if (go != null)
                    {
                        var siblings = h2_Unity.GetSiblings(go);
                        Selection.instanceIDs = siblings.Select(item => item.GetInstanceID()).ToArray();
                    }
                    return;
                }

                case h2_CommonSetting.CMD_SELECT_SCENE:
                {
                    var sgo = Selection.activeGameObject;
                    if (sgo == null) return;
                    h2_Unity.PingSceneContainsGO(sgo);
                    return;
                }
            }

            Debug.LogWarning("Unsupported command <" + cmd + ">");
        }

        public override float Draw(Rect r, GameObject go)
        {
            return 0;
        }
    }

    [Serializable]
    internal class h2_CommonSetting : h2_FeatureSetting
    {
        internal const string CMD_TOGGLE_ICON = "toggle_h2_icons";
        internal const string CMD_OPEN_SETTING = "open_h2_settings";
        internal const string CMD_COPY_NAME = "copy_go_name";
        internal const string CMD_COPY_HIERARCHY_NAME = "copy_go_hierarchy_name";

        internal const string CMD_CAMERA_LOOKTHROUGH = "camera_lookthrough";
        internal const string CMD_CAPTURE_SCENEVIEW = "capture_sceneview";

        internal const string CMD_GOTO_ROOT = "goto_root";
        internal const string CMD_GOTO_PARENT = "goto_parent";
        internal const string CMD_GOTO_CHILD = "goto_child";
        internal const string CMD_GOTO_SIBLING = "goto_sibling";

        internal const string CMD_SELECT_SIBLING = "select_sibling";
        internal const string CMD_SELECT_SCENE = "select_scene";

        const string TITLE = "GLOBAL SETTINGS";
        const string DRAW_BG_LABEL = "Icon BG";
        const string DRAW_SELECTION_LABEL = "Selection BG (experimental)";
        const string ICON_SPACE_LABEL = "Icon space";
        const string ICON_OFFSET_LABEL = "Icon offset";

        static readonly string[] SHORTCUTS =
        {
            "Toggle Hierarchy2", CMD_TOGGLE_ICON, "%&#H",
            "Open Settings", CMD_OPEN_SETTING, "%&#O",
            "Copy Name", CMD_COPY_NAME, "#ENTER",
	        "Copy Hierarchy Name", CMD_COPY_HIERARCHY_NAME, "#&ENTER",
	        "Look through Camera", CMD_CAMERA_LOOKTHROUGH, "#L",
            "Capture SceneView", CMD_CAPTURE_SCENEVIEW, "#C",
            "Goto Root", CMD_GOTO_ROOT, string.Empty,
            "Goto Parent", CMD_GOTO_PARENT, "_[",
            "Goto Child", CMD_GOTO_CHILD, "_]",
            "Goto Next Siblings", CMD_GOTO_SIBLING, "_\\",
	        "Select Sibling", CMD_SELECT_SIBLING, "#1",
	        "Select Scene", CMD_SELECT_SCENE, "#~"
            //"Find References",		"find_references",			"",
            //"Find Usage",			"find_usage",				"",
        };

        [SerializeField] public bool drawBackground;
        [SerializeField] public bool drawSelectionBackground;
        [SerializeField] public int iconOffset;
        [SerializeField] public int iconSpace;

        internal override void Reset()
        {
            enableIcon = true;
            enableShortcut = true;

            iconSpace = 0;
            iconOffset = 0;
            drawBackground = true;
	        drawSelectionBackground = true;

            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);
            h2_Utils.DelayRepaintHierarchy();

#if H2_DEV
		Debug.Log("RESET COMMON");
#endif
        }

        public void DrawInspector()
        {
            if (DrawBanner(TITLE, true, false))
            {
                iconSpace = EditorGUILayout.IntSlider(ICON_SPACE_LABEL, iconSpace, 0, 16);
                iconOffset = EditorGUILayout.IntSlider(ICON_OFFSET_LABEL, iconOffset, -16, 64);
                drawBackground = EditorGUILayout.Toggle(DRAW_BG_LABEL, drawBackground);
                drawSelectionBackground = EditorGUILayout.Toggle(DRAW_SELECTION_LABEL, drawSelectionBackground);

                h2_GUI.DrawLine();
                DrawShortcut();
            }
        }
    }
}