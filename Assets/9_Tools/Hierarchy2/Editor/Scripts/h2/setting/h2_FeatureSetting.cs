using System;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    [Serializable]
    public class h2_FeatureSetting
    {
        internal static h2_FeatureSetting currentFeature;

        [SerializeField] internal bool enableIcon;
        [SerializeField] internal bool enableShortcut;
        [SerializeField] internal h2_AShortcut[] shortcuts;


        public virtual bool isReady
        {
            get { return (shortcuts != null) && (shortcuts.Length > 0); }
        }

        internal virtual void Reset()
        {
            throw new Exception("Must override Reset !");
        }

        // EDITOR ONLY
        internal bool DrawBanner(string title, bool header = false, bool shortcut = false)
        {
            if (!isReady)
            {
#if H2_DEV
			Debug.LogWarning(this + ": not ready !");
			#endif
                return false; //Reset();
            }

            if (currentFeature == null) currentFeature = this;
            GUILayout.Space(2f);
            if (h2_GUI.DrawBanner(currentFeature == this, title, -16f, 16f))
            {
                currentFeature = this;
                if (header) DrawHeader();
                if (shortcut) DrawShortcut();
                return true;
            }

            if (currentFeature == this)
            {
                currentFeature = null;
            }
            return false;
        }

        internal void DrawHeader()
        {
            if (!isReady) Reset();

            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            {
                enableIcon = GUILayout.Toggle(enableIcon, "Enable Icons");
                enableShortcut = GUILayout.Toggle(enableShortcut, "Enable Shortcuts");
            }
            GUILayout.EndHorizontal();
            h2_GUI.DrawLine();
        }

        internal void DrawShortcut()
        {
            if (!isReady) Reset();

            if (enableShortcut)
            {
                for (var i = 0; i < shortcuts.Length; i ++)
                {
                    shortcuts[i].Draw();
                }
                GUILayout.Space(8f);
            }
            else
            {
                EditorGUILayout.HelpBox("Shortcuts being disabled !", MessageType.Warning);
            }
        }
    }
}