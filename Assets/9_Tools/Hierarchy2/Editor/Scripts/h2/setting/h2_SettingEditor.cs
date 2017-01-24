using UnityEditor;
using UnityEngine;
#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif
namespace vietlabs.h2
{
    [CustomEditor(typeof(h2_Setting))]
    public class h2_SettingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var s = (h2_Setting) target;

            Profiler.BeginSample("h2.Setting.OnGUI");
            EditorGUI.BeginChangeCheck();
            {
                //s.Check2Reset();
                s.Common.DrawInspector();
                s.SceneViewHL.DrawInspector();
	            s.ParentIndicator.DrawInspector();
                s.Script.DrawInspector();
                s.Lock.DrawInspector();
                s.Active.DrawInspector();
                s.Prefab.DrawInspector();
                s.Static.DrawInspector();
                s.Combine.DrawInspector();
                s.GOIcon.DrawInspector();
                s.Tag.DrawInspector();
                s.Layer.DrawInspector();


                //Debug.Log("A");

                s.Component.DrawInspector();

                //Debug.Log("B");

                GUILayout.FlexibleSpace();
                s.palette.Draw();
                //s.Static.Draw(s.previewAllStates);
                //s.Lock.Draw(s.previewAllStates);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                h2_Utils.DelayRepaintHierarchy();
            }
            Profiler.EndSample();
        }
    }
}