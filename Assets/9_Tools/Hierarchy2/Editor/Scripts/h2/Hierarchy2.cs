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
    [InitializeOnLoad]
    public class Hierarchy2
    {
        const string VERSION = "2.1.0";

        private static readonly Type HDType;
        private static float _width;

        // temp variable to fix selection not update on MouseDown
        static int lastMouseDownOn;
        static bool lastMouseDownSingle;
        static int firstInst;

        static float lastRepaintTime;

        static Dictionary<int, int> widthCache;


        static int lastDrawIcon;


        static int _inspectorLocked;

        // ----------------- SETTINGS ----------------------
        //static List<h3_Icon> IconList;
        static List<h2_Icon> IconList2;

        static Hierarchy2()
        {
	        HDType = h2_Reflection.GetTypeByName("vietlabs.HierarchyDraw");
	        
            if (HDType != null)
            {
                var addFunc = HDType.GetMethod("Add");
                addFunc.Invoke(null, new object[] {"Hierarchy3", (Func<int, Rect, Rect>) OnHierarchyDraw});

                var priorFunc = HDType.GetMethod("SetPriority");
                priorFunc.Invoke(null, new object[] {"Hierarchy3", -1});
            }
            else
            {
                EditorApplication.hierarchyWindowItemOnGUI -= HierarchyItemCB;
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
            }

            h2_Selection.Register_OnSelectionChange(OnSelectionChange);

            EditorApplication.update -= PeriodRepaint;
            EditorApplication.update += PeriodRepaint;

            Undo.undoRedoPerformed -= ForceRefresh;
            Undo.undoRedoPerformed += ForceRefresh;
        }

        static void ForceRefresh()
        {
            h2_Utils.RepaintHierarchy();
        }

        static void PeriodRepaint()
        {
            var dur = h2_Unity.focusingInspector ? 0.1f : 1f;
            if (Time.realtimeSinceStartup - lastRepaintTime < dur) return;
            checkInspectorLock();
            h2_Utils.RepaintHierarchy();
        }

        static void OnSelectionChange(Object[] gameObjects)
        {
            h2_Utils.DelayRepaintHierarchy();
        }

        static Rect OnHierarchyDraw(int instID, Rect r)
        {
            HierarchyItemCB(instID, r);
            return new Rect(r.x, r.y, r.width - _width, r.height);
        }

        private static int GetWidth(int instID)
        {
            if (widthCache == null) widthCache = new Dictionary<int, int>();
            int result;
            if (widthCache.TryGetValue(instID, out result))
            {
                return result;
            }
            return 0;
        }

        private static void SetWidth(int instID, int value)
        {
            if (!widthCache.ContainsKey(instID))
            {
                widthCache.Add(instID, value);
            }
            else
            {
                widthCache[instID] = value;
            }
        }

        private static void HierarchyItemCB(int instID, Rect r)
        {
#if H2_DEV
			Profiler.BeginSample("Hierarchy2.HierarchyItemCB");
#endif

            if (h2_Setting.current == null) return;

            var go = (GameObject) EditorUtility.InstanceIDToObject(instID);
            if (go == null) return;

            // REFRESH ICON LIST - IF NECESSARY
            if (IconList2 == null) Refresh();
	        if (h2_Lazy.RESET_STAMP != h2_Setting.RESET_STAMP)
            {
#if H2_DEV
				Profiler.BeginSample("Hierarchy2.HierarchyItemCB-RefreshSettings");
#endif
                for (var i = 0; i < IconList2.Count; i++)
                {
                    IconList2[i].RefreshSettings();
                }
#if H2_DEV
				Profiler.EndSample();
#endif

		        h2_Lazy.RESET_STAMP = h2_Setting.RESET_STAMP;
            }

            // REFRESH EVENTS - IF NECCESSARY
#if H2_DEV
			Profiler.BeginSample("Hierarchy2.HierarchyItemCB-RefreshEvent");
#endif

            var cs = h2_Setting.current.Common;
            var e = Event.current;
            //var isMouse = e.isMouse;

            lastRepaintTime = Time.realtimeSinceStartup;

            if (h2_Lazy.eventType != e.type || (e.type == EventType.Repaint && instID == firstInst))
            {
                h2_Lazy.eventType = e.type;
                h2_Lazy.isRepaint = e.type == EventType.Repaint;
                h2_Lazy.isMouseDown = e.type == EventType.MouseDown;
                h2_Lazy.isPlaying = EditorApplication.isPlaying;
                h2_Lazy.isPro = EditorGUIUtility.isProSkin;
	            h2_Lazy.isFocus = h2_Unity.focusingHierarchy;

                var lastRename = h2_Lazy.isRenaming;
                h2_Lazy.isRenaming = h2_Unity.IsRenaming;
                if (lastRename != h2_Lazy.isRenaming)
                {
                    h2_Utils.DelayRepaintHierarchy();
                }

	            h2_Selection.CheckIfSelectionChanged();
	            
	            

                if (cs.drawBackground)
                {
	                h2_Lazy.bgColor = h2_Color.GetBGColor(false, h2_Lazy.isFocus, h2_Lazy.isPro);

                    if (cs.drawSelectionBackground)
                    {
	                    h2_Lazy.bgColorSelected = h2_Color.GetBGColor(true, h2_Lazy.isFocus, h2_Lazy.isPro);
                    }
                }

                if (lastMouseDownOn != 0 && e.type == EventType.mouseUp)
                {
                    lastMouseDownOn = 0;
                }

	            if (!h2_Lazy.isRepaint && e.type != EventType.used)
                {
                    firstInst = instID;
                }

                lastDrawIcon = 0;
                for (var i = 0; i < IconList2.Count; i++)
                {
                	var ic = IconList2[i];
	                if (ic.setting == null || ic.setting.enableIcon)
                    {
                        lastDrawIcon = i + 1;
                    }
                }
	            
	            if ((e.isMouse || e.type == EventType.used) && h2_Unity.HierarchyWindow.wantsMouseMove)
                {
                    h2_Utils.DelayRepaintHierarchy();
                }
	            
	            //Debug.Log(instID + ":" + e + ":" + h2_Lazy.isRepaint + ":" + h2_Lazy.isRenaming + ":" + cs.drawBackground);
            }

#if H2_DEV
			Profiler.EndSample();
#endif

#if H2_DEV
			Profiler.BeginSample("Hierarchy2.HierarchyItemCB-DoDraw");
#endif

            if (cs.enableShortcut && !h2_Lazy.isRenaming)
            {
                h2_Shortcut.Api.Check();
            }

            if (!cs.enableIcon)
            {
                _width = 0;
#if H2_DEV
			Profiler.EndSample();
			Profiler.EndSample();
#endif
                return;
            }

            var goW = GetWidth(instID);
            var bgRect = new Rect(r.x + r.width - goW, r.y, goW, r.height);
            var isLocked = instID == _inspectorLocked;


	        if (h2_Lazy.isRepaint)
            {
                if (isLocked)
                {
                    var rr = new Rect(0, bgRect.y, Screen.width, bgRect.height);
                    h2_GUI.SolidColor(rr, new Color32(255, 0, 0, 64));
                }
                else if (cs.drawBackground)
                {
                    // DRAW CORRECT BACKGROUND FOR ACTIVE ITEM
                    if (!h2_Lazy.isRenaming || h2_Selection.gameObject != go)
                    {
                        if (cs.drawSelectionBackground)
                        {
                            var matched = false;
                            if (lastMouseDownOn != 0)
                            {
                                if (lastMouseDownSingle)
                                {
                                    matched = instID == lastMouseDownOn;
                                }
                                else
                                {
                                    matched = (instID == lastMouseDownOn) || h2_Selection.Contains(go);
                                }
                            }
                            else
                            {
                                matched = h2_Selection.Contains(go);
                            }
                            h2_GUI.SolidColor(bgRect, matched ? h2_Lazy.bgColorSelected : h2_Lazy.bgColor);
                        }
                        else
                        {
                            h2_GUI.SolidColor(bgRect, h2_Lazy.bgColor);
                        }
                    }
                }
            }

            float spc = cs.iconSpace;
            _width = r.width;
            r.width -= cs.iconOffset;

            var n = lastDrawIcon;

            for (var i = 0; i < n; i++)
            {
                var ico = IconList2[i];
                if (ico.setting != null && !ico.setting.enableIcon)
                {
                    continue;
                }

                var dw = ico.Draw(r, go);

                if (i == n - 1)
                {
                    if (dw > 0)
                    {
                        r.width -= dw + spc;
                    }
                }
                else
                {
                    r.width -= ico.MaxWidth + spc;
                }
            }

            var ww = _width - r.width;
            if (h2_Lazy.isRepaint && (goW != ww))
            {
                SetWidth(instID, (int) ww);
                h2_Utils.DelayRepaintHierarchy();
            }

            _width = ww;

            if (e.type == EventType.MouseDown && (e.button == 0))
            {
                if (bgRect.Contains(e.mousePosition))
                {
                    //Debug.Log("Swallow mouse : ");	
                    e.Use();
                }
                else if (cs.drawSelectionBackground)
                {
                    var rowRect = new Rect(0, r.y, r.x + r.width, r.height);
                    if (rowRect.Contains(e.mousePosition))
                    {
                        if (go.transform.childCount > 0)
                        {
                            var arrowRect = new Rect(r.x - 16f, r.y, 16f, 16f);
                            //h2_GUI.SolidColor(arrowRect, Color.white);
                            if (arrowRect.Contains(e.mousePosition))
                            {
                                lastMouseDownOn = 0;
#if H2_DEV
							Profiler.EndSample();
							Profiler.EndSample();
#endif

                                return;
                            }
                        }

                        lastMouseDownOn = instID;
                        lastMouseDownSingle = !e.control && !e.shift;
                    }
                }
            }

#if H2_DEV
			Profiler.EndSample();
			Profiler.EndSample();
#endif

            //Profiler.EndSample();

            //
            //if (IconList == null) Refresh();

            //var info = h3_Info.Get(instID, true);
            //if (info == null) {
            //	// CAN BE NULL AS OF UNITY_5_4 MULTISCENE
            //	//Debug.Log("Info is null !");
            //	return;
            //}


            //var evtType = Event.current.type;
            //var isRepaint = evtType == EventType.Repaint;
            //var s = h3_Setting.Settings;
            //var willRepaint = false;
            //var effectiveRect = h2_Utils.subRectRight(r, info.lastW + s.iconSpacing + s.iconPadding);

            //if (isRepaint && s.clearBackground && !info.inSelection)
            //{
            //	h2_GUI.SolidColor(effectiveRect, h3_Setting.CurrentTheme.BackgroundColor);
            //}

            //r.xMax -= s.iconPadding;
            //var totalW = 0f;

            //for (var i = 0;i < IconList.Count; i++){
            //	var icon = IconList[i];
            //	var oldW = icon.MaxWidth;	
            //	var newW = icon.Draw(r, isRepaint, info);
            //	totalW += (i == IconList.Count-1) ? newW : icon.MaxWidth;
            //	r.xMax -= icon.MaxWidth;

            //	if (oldW < icon.MaxWidth && !isRepaint) {
            //		willRepaint = true;
            //	}
            //}

            //if (isRepaint) info.lastW = totalW;

            //if (info.dIndex == 0 && !isRepaint){
            //	_width = info.lastW;
            //} else if (info.lastW > _width) {
            //	_width = info.lastW;
            //}

            //if (willRepaint) h2_Utils.DelayRepaintHierarchy();
        }

        static void checkInspectorLock()
        {
            var inspector = h2_Unity.InspectorWindow;
            if (inspector == null) return;

            _inspectorLocked = -1;
            var isLocked = (bool) h2_Reflection.GetProperty(inspector, "isLocked");

            if (isLocked)
            {
                var tracker = h2_Reflection.GetField(inspector, "m_Tracker");
                var list = h2_Reflection.GetProperty(tracker, "activeEditors");

#if UNITY_4_3
			var editors = (Object[])list;
			if (editors.Length > 0) {
				for (var i = 0;i < editors.Length; i++) {
					var e = editors[i] as Editor;
					if (e != null) {
						_inspectorLocked = e.target.GetInstanceID();
						return;
					}
				}
			}
#else
                var editors = (Editor[]) list;
                if (editors.Length > 0)
                {
                    for (var i = 0; i < editors.Length; i++)
                    {
                        if (editors[i].target != null)
                        {
                            _inspectorLocked = editors[i].target.GetInstanceID();
                            return;
                        }
                    }
                }
#endif
            }
        }

        public static void Refresh()
        {
            if (IconList2 == null)
            {
                IconList2 = new List<h2_Icon>();
            }
            else
            {
                IconList2.Clear();
            }

            IconList2.Add(new h2_Common());
            IconList2.Add(new h2_ParentIndicator());
            IconList2.Add(new h2_SceneViewHL());
            IconList2.Add(new h2_Script());
            IconList2.Add(new h2_Lock());
            IconList2.Add(new h2_Active());
            IconList2.Add(new h2_Static());
            IconList2.Add(new h2_Combine());
            IconList2.Add(new h2_GOIcon());
            IconList2.Add(new h2_Prefab());
            IconList2.Add(new h2_Tag());
            IconList2.Add(new h2_Layer());
            IconList2.Add(new h2_Component());


            //check if in bgRect : use event

            //var features = h3_Setting.CurrentFeatures;
            //if (IconList == null)
            //{
            //	IconList = new List<h3_Icon>();
            //}
            //else
            //{
            //	IconList.Clear();
            //}

            //if (features.Script) IconList.Add(new h3_ScriptIndicator());
            //if (features.Active) IconList.Add(new h3_Active());
            //if (features.Static) IconList.Add(new h3_Static());
            //if (features.Combine) IconList.Add(new h3_Combine());
            //if (features.Layer) IconList.Add(new h3_Layer());
            ////if (features.Tag) IconList.Add(new h3_Tag());
            //if (features.Component) IconList.Add(new h3_Component());

            //RefreshTheme();                                                  
            //h3_Info.Map.Clear();
        }

        //}
        //	//vlb_Shortcut.Api.Reset(h3_Setting.ShortCuts);
        //	}                                            
        //		IconList[i].RefreshTheme();
        //	for (var i = 0;i < IconList.Count; i++){

        //public static void RefreshTheme(){
    }
	
	
	internal static class h2_Lazy 
	{
		public static int RESET_STAMP = 0;
		
		public static EventType eventType;
		public static bool isRepaint;
		public static bool isMouseDown;
		public static bool isPlaying;
		public static bool isPro;
		public static bool isFocus;
		public static bool isRenaming;
		public static Color bgColor;
		public static Color bgColorSelected;
	}
}





/* 


ArgumentOutOfRangeException: Argument is out of range.
Parameter name: index
  at System.Collections.Generic.List`1[UnityEditor.TreeViewItem].get_Item (Int32 index) [0x0000c] in /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Collections.Generic/List.cs:633 
  at UnityEditor.GameObjectTreeViewDataSource.GetItem (Int32 row) [0x00000] in C:\buildslave\unity\build\Editor\Mono\GUI\TreeView\GameObjectTreeViewDataSource.cs:131 
  at UnityEditor.TreeView.GetItemAndRowIndex (Int32 id, System.Int32& row) [0x00018] in C:\buildslave\unity\build\Editor\Mono\GUI\TreeView\TreeView.cs:771 
  at UnityEditor.TreeView.KeyboardGUI () [0x00164] in C:\buildslave\unity\build\Editor\Mono\GUI\TreeView\TreeView.cs:859 
  at UnityEditor.TreeView.OnGUI (Rect rect, Int32 keyboardControlID) [0x001bb] in C:\buildslave\unity\build\Editor\Mono\GUI\TreeView\TreeView.cs:520 
  at UnityEditor.SceneHierarchyWindow.DoTreeView (Single searchPathHeight) [0x00016] in C:\buildslave\unity\build\Editor\Mono\SceneHierarchyWindow.cs:505 
  at UnityEditor.SceneHierarchyWindow.OnGUI () [0x000a1] in C:\buildslave\unity\build\Editor\Mono\SceneHierarchyWindow.cs:320 
  at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (object,object[],System.Exception&)
  at System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x000d0] in /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:222 
 
 
 */