using System;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    internal class h2_GOIcon : h2_Icon
    {
        public h2_GOIcon() : base("h2_go_icon")
        {
        }

        public override void RefreshSettings()
        {
            setting = h2_Setting.current.GOIcon;
            MaxWidth = 20f;
        }

        public override float Draw(Rect r, GameObject go)
        {
            if (go == null)
            {
                Debug.LogWarning("GO should not be null or empty !");
                return 0;
            }

            if (Event.current.type != EventType.repaint)
            {
                return 0;
            }

            var so = new SerializedObject(go);
            so.Update();

            var property = so.FindProperty("m_Icon");
            if (property == null) return 0;

            var tex = (Texture2D) property.objectReferenceValue;
            if (tex == null) return 0;


            //var scl = 1f;
            //var h = tex.height;

            //if (h > 16f)
            //{
            //    scl = 16f / h;
            //    h = 16;
            //}

            //var w = tex.width * scl;
            //var x = r.x + r.width - (MaxWidth - w)/2f;
            //MaxWidth = Mathf.Max(w, MaxWidth);
            var h = tex.height > 16f ? 16f : tex.height;
            var w = h*tex.width/tex.height;

            var x = r.x + r.width - MaxWidth + (MaxWidth - w)/2f;
            GUI.DrawTexture(new Rect(x, r.y, w, h), tex);
            return w;
        }

        // ----------------------------- SHORTCUT HANDLER -----------------------------

        protected override void RunCommand(string cmd)
        {
            switch (cmd)
            {
                case h2_GOIconSetting.CMD_TOGGLE_ICO:
                {
                    setting.enableIcon = !setting.enableIcon;
                    EditorUtility.SetDirty(h2_Setting.current);
	                //AssetDatabase.SaveAssets();
	                h2_Utils.DelaySaveAssetDatabase();
                    return;
                }

                default:
                    Debug.Log("Unhandled command <" + cmd + ">");
                    break;
            }
        }
    }

    [Serializable]
    internal class h2_GOIconSetting : h2_FeatureSetting
    {
        internal const string CMD_TOGGLE_ICO = "toggle_ico";

        const string TITLE = "GAMEOBJECT ICON";

        static readonly string[] SHORTCUTS =
        {
            "Toggle GameObject icons", CMD_TOGGLE_ICO, "#%&I"
        };

        internal override void Reset()
        {
            enableIcon = false;
            enableShortcut = true;
            shortcuts = h2_Shortcut.FromStrings(SHORTCUTS);

#if H2_DEV
		Debug.Log("RESET GAMEOBJECT ICON");
#endif
        }

        internal void DrawInspector()
        {
            DrawBanner(TITLE, true, true);
        }
    }
}