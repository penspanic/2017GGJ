using System;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#endif

namespace vietlabs.h2
{
    public class h2_Icon
    {
        

        public float MaxWidth = 16f;
        public h2_FeatureSetting setting;

        public h2_Icon(string id, float width = 16f)
        {
            MaxWidth = width;
            RefreshSettings();
	        h2_Shortcut.Api.AddHandler(id, RunCommand, GetShortcuts);
        }

        public virtual void RefreshSettings()
        {
            throw new Exception("Must override ! ");
        }

        internal h2_AShortcut[] GetShortcuts()
        {
            if (setting == null || !setting.enableShortcut || !h2_Unity.focusingHierarchy)
            {
                //Debug.Log("GetShortCut :: " + setting + ":" + h2_Unity.focusingHierarchy);
                return null;
            }
            return setting.shortcuts;
        }

#if H2_DEV
	internal int EndProfileReturn(int value){
		Profiler.EndSample();
		return value;
	}
#endif

        //public bool enabled
        //{
        //	get { 
        //		if (h2_Setting.current == null) return false;
        //		return setting.enableIcon;
        //	}
        //}

        // ----------------------- OVERRIDE REQUIRED --------------------------------

        protected virtual void RunCommand(string cmd)
        {
        }

        //virtual protected h2_FeatureSetting setting { get { return null; }}
        public virtual float Draw(Rect r, GameObject go)
        {
            return MaxWidth;
        }
    }
}