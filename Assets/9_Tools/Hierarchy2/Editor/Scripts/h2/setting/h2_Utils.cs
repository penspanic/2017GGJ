using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    internal static class h2_Utils
    {
        //public static void SetExpand(h3_Info info, bool value){

        //	var flags	= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        //	#if UNITY_5_0_OR_NEWER

        //	var tree	= WindowX.Hierarchy.xGetField("m_TreeView", null, flags);
        //	var data	= tree.xGetProperty("data", null, flags);
        //	var tvi		= data.xInvoke("FindItem", null, null, new object[]{ info.instID } );

        //	try
        //	{
        //		data.xInvoke("SetExpanded", TypeX.ITreeViewDataSourceT, new [] {TypeX.GameObjectTreeViewItemT, typeof(bool)}, null, new [] { tvi, value } );
        //	}
        //	catch (Exception e)
        //	{
        //		Debug.LogWarning("Unity changes - Need update ! \n" + e);
        //	} 
        //	#elif UNITY_4_5_OR_NEWER

        //	#else


        //}

        static readonly Dictionary<string, string> ShortenMap = new Dictionary<string, string>();

        static bool willRepaintHierarchy;
        static bool willRepaintInspector;


        public static void ForceRefreshHierarchy()
        {
            Debug.LogWarning("Force Hierarcy refresh !");
            //h2_Utils.RootTransforms.ForEach(
            //	t => {
            //		if (t == null) return;

            //		t.gameObject.xForeachChild(
            //			child => {
            //				child.xToggleFlag(HideFlags.NotEditable);
            //				child.xToggleFlag(HideFlags.NotEditable);
            //			}, true);

            //		WindowX.Hierarchy.Repaint();
            //	}
            //);
        }
	    
	    static private bool _willSaveAssets;
	    public static void DelaySaveAssetDatabase()
	    {
	    	if (_willSaveAssets) return;
	    	
	    	_willSaveAssets = true;
	    	EditorApplication.update -= DoSaveAsset;
	    	EditorApplication.update += DoSaveAsset;
	    }
	    
	    static void DoSaveAsset(){
	    	_willSaveAssets = false;
	    	EditorApplication.update -= DoSaveAsset;
	    	AssetDatabase.SaveAssets();
	    }


        public static string GetBriefName(string name)
        {
            var str = string.Empty;
            for (var i = 0; i < name.Length; i++)
            {
	            if (char.IsLower(name[i]) || char.IsWhiteSpace(name[i])) continue;
		        str += name[i];
            }
            return string.IsNullOrEmpty(str) ? name[0].ToString() : str;
        }

        public static string GetHierarchyName(Transform t)
        {
            //TODO : cache result ?
            var result = new List<string>();
            while (t != null)
            {
                result.Add(t.name);
                t = t.parent;
            }
            result.Reverse();
            return string.Join("/", result.ToArray());
        }

        public static string GetShortenName(string name)
        {
            string shortenName;
            if (!ShortenMap.TryGetValue(name, out shortenName))
            {
                shortenName = GetBriefName(name);
                ShortenMap.Add(name, shortenName);
                //Debug.Log("Add : " + name + ":" + shortenName);
            }

            return shortenName;
        }


        public static Rect subRectRight(Rect r, float w)
        {
            return new Rect(r.x + r.width - w, r.y, w, r.height);
        }

        public static Rect subRectRight(ref Rect r, float w, float spc = 0)
        {
            //SubRectRightRef
            var rSub = new Rect(r.x + r.width - w, r.y, w, r.height);
            r.width -= w + spc;
            return rSub;
        }

        public static string GetHierarchyName(Transform t, Transform relativeParent)
        {
            if (t == null)
                return string.Empty;

            var p = t.parent;
            var result = t.name;

            while (p != null && p != relativeParent)
            {
                result = p.name + "/" + result;
                p = p.parent;
            }

            if (p == relativeParent)
            {
                result = "./" + result;
            }

            return result;
        }

        public static void DelayRepaintHierarchy()
        {
            if (willRepaintHierarchy) return;
	        
	        //Debug.Log("willRepaint Triggered !");
            willRepaintHierarchy = true;
	        
	        EditorApplication.update-=RepaintHierarchy;
            EditorApplication.update += RepaintHierarchy;
        }


        public static void DelayRepaintInspector()
        {
            if (willRepaintInspector) return;
            
	        willRepaintInspector = true;
	        EditorApplication.update -=RepaintInspector;
            EditorApplication.update += RepaintInspector;
        }

        internal static void RepaintHierarchy()
	    {
		    EditorApplication.update -= RepaintHierarchy;
		    if (!willRepaintHierarchy) return;
		    
		    //Debug.Log("Repaint Hierarchy Triggered ! " + willRepaintHierarchy);
		    willRepaintHierarchy = false;
            var hWindow = h2_Unity.HierarchyWindow;
            if (hWindow != null) hWindow.Repaint();
        }

        internal static void RepaintInspector()
	    {
		    EditorApplication.update -= RepaintInspector;
		    if (!willRepaintInspector) return;
		    
		    //Debug.Log("Repaint Inspector Triggered ! " + willRepaintInspector);
		    willRepaintInspector = false;
		    
            var hWindow = h2_Unity.InspectorWindow;
            if (hWindow != null) hWindow.Repaint();
        }
    }


    internal static class h2Extern
    {
        internal static bool IsMultiScene(GameObject go)
        {
            return go != null && IsMultiScene(go.transform);
        }

        internal static bool IsMultiScene(Transform t)
        {
            if (t == null || t.parent != null) return false; //Multiscene Objects must be Root
            var c = t.GetComponent("Multiscene");
            return c != null;
        }
    }
}