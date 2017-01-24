#if UNITY_5_3_OR_NEWER //Since Unity 5.3.4
#define UNITY_4_3_OR_NEWER
#define UNITY_4_4_OR_NEWER
#define UNITY_4_5_OR_NEWER
#define UNITY_4_6_OR_NEWER
#define UNITY_4_7_OR_NEWER
#define UNITY_5_0_OR_NEWER
#define UNITY_5_1_OR_NEWER
#define UNITY_5_2_OR_NEWER
#else
    #if UNITY_5
	    #define UNITY_4_3_OR_NEWER
        #define UNITY_4_4_OR_NEWER
        #define UNITY_4_5_OR_NEWER
        #define UNITY_4_6_OR_NEWER
        #define UNITY_4_7_OR_NEWER
	
        #if UNITY_5_0 
            #define UNITY_5_0_OR_NEWER
	    #elif UNITY_5_1
		    #define UNITY_5_0_OR_NEWER
		    #define UNITY_5_1_OR_NEWER
	    #elif UNITY_5_2
		    #define UNITY_5_0_OR_NEWER
		    #define UNITY_5_1_OR_NEWER
		    #define UNITY_5_2_OR_NEWER
        #elif UNITY_5_3
		    #define UNITY_5_0_OR_NEWER
		    #define UNITY_5_1_OR_NEWER
		    #define UNITY_5_2_OR_NEWER
            #define UNITY_5_3_OR_NEWER
	    #endif
    #else
        #if UNITY_4_3
            #define UNITY_4_3_OR_NEWER
        #elif UNITY_4_4
            #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
        #elif UNITY_4_5    
		    #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
            #define UNITY_4_5_OR_NEWER
        #elif UNITY_4_6
		    #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
            #define UNITY_4_5_OR_NEWER
            #define UNITY_4_6_OR_NEWER
        #elif UNITY_4_7
		    #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
            #define UNITY_4_5_OR_NEWER
            #define UNITY_4_6_OR_NEWER
            #define UNITY_4_7_OR_NEWER
        #endif
    #endif
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;

#endif

namespace vietlabs.h2
{
    public class h2_Unity
    {
	    
	    #if UNITY_4_5_OR_NEWER
	    public const int parentIndicatorW = 14;
	    #else
	    public const int parentIndicatorW = 16;
	    #endif
	    
	    public static int ParentIndicatorX {
	    	get {
#if UNITY_5_5_OR_NEWER
	    		return 14;
#elif UNITY_5_3_OR_NEWER
	    	    return SceneManager.sceneCount > 1 ? 14 : 0;
#else
                return 0;
	    		#endif
	    	}
	    }
	    
	    
	    
	    
        // ------------------------------ ASSETS -----------------------------------------

        private static bool tryGetIsDirty;
        private static MethodInfo mIsDirty;

        public static bool isDirty(int instID)
        {
            if (tryGetIsDirty && mIsDirty == null) return false;

            tryGetIsDirty = true;
            mIsDirty = typeof(EditorUtility).GetMethod("IsDirty", BindingFlags.Static | BindingFlags.NonPublic);

            if (mIsDirty != null)
            {
                return (bool) mIsDirty.Invoke(null, new object[] {instID});
            }
            Debug.LogWarning("Unity Changed : EditorUtility.IsDirty(int instID) not exist");
            return false;
        }
	    
	    public static T LoadAssetAtPath<T>(string path) where T : Object {
		#if UNITY_5_1_OR_NEWER
			return AssetDatabase.LoadAssetAtPath<T>(path);
		#else
		    return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
		#endif
	    }

        // ---------------------------- HIERARCHY ---------------------------

        internal static void FocusInScene(GameObject go)
        {
            if (go == null) return;
            var last = SceneView.lastActiveSceneView;
            if (last != null) last.LookAt(go.transform.position);
        }

        internal static string[] GetLayerNames()
        {
            return (string[]) InternalEditorUtilityT
                .GetProperty("layers")
                .GetValue(null, null);
        }

        internal static void PingSceneContainsGO(GameObject go)
        {
            if (go == null) return;

            var scenePath = 
#if UNITY_5_3_OR_NEWER
            FindSceneContains(go.transform).path;
#else
	        EditorApplication.currentScene;
#endif
	        
            EditorGUIUtility.PingObject(LoadAssetAtPath<Object>(scenePath));
        }

        //fieldInfo cache for faster access
        static FieldInfo[] renameFields;

        internal static bool IsRenaming
        {
            get
            {
                var w = HierarchyWindow;

                if (w == null)
                {
                    Debug.LogWarning("Hierarchy window should not be null !");
                    return false;
                }

                if (renameFields == null)
                {
                    var flags = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public |
                                BindingFlags.NonPublic;
                    renameFields = new[]
                    {
#if UNITY_4_5_OR_NEWER
                        SceneHierarchyWindowT.GetField("m_TreeViewState", flags),
                        TreeViewStateT.GetField("m_RenameOverlay", flags),
                        RenameOverlayT.GetField("m_IsRenaming", flags)
#else
					BaseProjectWindowT.GetField("m_RealEditNameMode", flags)
#endif
                    };
                }

#if UNITY_4_5_OR_NEWER
                {
                    //Get the private TreeViewState m_TreeViewState from SceneHierarchyWindow
                    var m_TreeViewState = renameFields[0].GetValue(w);
                    //Debug.Log("m_TreeViewState --> "+ m_TreeViewState);

                    // Get the private RenameOverlay m_RenameOverlay from TreeViewState
                    var m_RenameOverlay = renameFields[1].GetValue(m_TreeViewState);
                    //Debug.Log("m_RenameOverlay --> "+ m_TreeViewState);

                    var isRenaming = (bool) renameFields[2].GetValue(m_RenameOverlay);
                    //Debug.Log("m_IsRenaming ---> "+ isRenaming);

                    return isRenaming;
                }
#else
			{
				// UNITY 4 :: private BaseProjectWindow.NameEditMode m_RealEditNameMode;
				var mode = (int)renameFields[0].GetValue(w);
				return mode != 0;
			}
#endif
            }
        }

        internal static void SetExpand(GameObject go, bool expand)
        {
            if (go.transform.childCount == 0) return;

            // TEMP FIX - UNITY KEEPS CHANGING 
            if (expand)
            {
                EditorGUIUtility.PingObject(go.transform.GetChild(0));
            }
            else
            {
                var tmp = new GameObject();
                tmp.hideFlags = HideFlags.DontSave;
                Object.DestroyImmediate(tmp);
            }
        }

#if UNITY_5_3_OR_NEWER
        internal static Scene FindSceneContains(Transform t)
        {
            if (t == null)
            {
                return SceneManager.GetSceneAt(0);
            }

            var p = t;
            while (p.parent != null) p = p.parent;
            var go = t.gameObject;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                if (roots.Contains(go))
                {
                    return scene;
                }
            }

            Debug.LogWarning("Something wrong, can not find any scene contains GO <" + t.gameObject + ">");
            return SceneManager.GetSceneAt(0);
        }
        
        internal static GameObject[] GetRootGOs()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                return SceneManager.GetActiveScene().GetRootGameObjects();
            }

            var t = go.transform;
            while (t.parent != null)
            {
                t = t.parent;
            }

            //if (_lastSearchScene != null)
            //{
            //	Debug.Log(_lastSearchScene.name);
            //	var roots = _lastSearchScene.GetRootGameObjects();
            //	if (roots.Contains(go)) return roots;
            //}

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                if (roots.Contains(go))
                {
                    //_lastSearchScene = scene;
                    return roots;
                }
            }

            Debug.LogWarning("Something wrong, should never be here !");
            return null;
        }
#else
	
	internal static GameObject[] GetRootGOs()
	{
		var result = new List<GameObject>();
		result.AddRange(internalGetRootGOs());
		return result.ToArray();
	}

	internal static IEnumerable<GameObject> internalGetRootGOs(){
		var prop = new HierarchyProperty(HierarchyType.GameObjects);
		var expanded = new int[0];
		while (prop.Next(expanded))
		{
			//Debug.Log(prop.pptrValue + ":" + prop.instanceID + ":" + prop.isValid + ":" + prop.name + ":" + prop.isFolder);
			if (prop.pptrValue != null) yield return prop.pptrValue as GameObject;
		}
	}
#endif

        internal static List<Transform> RootTransforms
        {
            get
            {
                var result = new List<Transform>();
                var gos = GetRootGOs();

                for (var i = 0; i < gos.Length; i ++)
                {
                    result.Add(gos[i].transform);
                }
                return result;
            }
        }

        internal static List<GameObject> GetSiblings(GameObject go)
        {
            var p = go.transform.parent;
            var result = new List<GameObject>();

            if (p != null)
            {
                result.AddRange(from Transform t in go.transform.parent select t.gameObject);
            }
            else
            {
                result.AddRange(GetRootGOs());
            }
            return result;
        }


        // ------------------------------ SCENES -----------------------------------------

        internal static string ActiveScene
        {
            get
            {
#if UNITY_5_3_OR_NEWER
                return SceneManager.GetActiveScene().path;
#else
			    return EditorApplication.currentScene;
#endif
            }
        }

        // ------------------------------ EDITOR WINDOWS -----------------------------------------

        public static void SetWindowTitle(EditorWindow window, string title)
        {
#if UNITY_5_1_OR_NEWER
            window.titleContent = new GUIContent(title);
#else
			window.title = title;
#endif
        }


        // ----------------------------- REFLECTIONS ----------------------------------

        internal static Type _BaseProjectWindowT;

        public static Type BaseProjectWindowT
        {
            get
            {
                return _BaseProjectWindowT ?? (
	                _BaseProjectWindowT = h2_Reflection.GetTypeByName("UnityEditor.BaseProjectWindow")
                    );
            }
        }

        internal static Type _TreeViewT;

        public static Type TreeViewT
        {
            get
            {
                return _TreeViewT ?? (
                    _TreeViewT = h2_Reflection.GetTypeByName(
#if UNITY_5_5_OR_NEWER
                        "UnityEditor.IMGUI.Controls.TreeView"
#else
                        "UnityEditor.TreeView"
#endif
                        )
                    );
            }
        }

        internal static Type _TreeViewStateT;

        public static Type TreeViewStateT
        {
            get
            {
                return _TreeViewStateT ?? (
                    _TreeViewStateT = h2_Reflection.GetTypeByName(
#if UNITY_5_5_OR_NEWER
                        "UnityEditor.IMGUI.Controls.TreeViewState"
#else
                        "UnityEditor.TreeViewState"
#endif
                    )
                );
            }
        }

        internal static Type _RenameOverlayT;

        public static Type RenameOverlayT
        {
            get
            {
                return _RenameOverlayT ?? (
                    _RenameOverlayT = h2_Reflection.GetTypeByName("UnityEditor.RenameOverlay")
                    );
            }
        }

        internal static Type _SceneHierarchyWindowT;

        public static Type SceneHierarchyWindowT
        {
            get
            {
                return _SceneHierarchyWindowT ?? (
                    _SceneHierarchyWindowT = h2_Reflection.GetTypeByName("UnityEditor.SceneHierarchyWindow")
                    );
            }
        }


        internal static Type _InternalEditorUtilityT;

        public static Type InternalEditorUtilityT
        {
            get
            {
                return _InternalEditorUtilityT ?? (
                    _InternalEditorUtilityT = h2_Reflection.GetTypeByName("UnityEditorInternal.InternalEditorUtility")
                    );
            }
        }

        internal static Type _ITreeViewDataSourceT;

        public static Type ITreeViewDataSourceT
        {
            get
            {
                return _ITreeViewDataSourceT ?? (
                    _ITreeViewDataSourceT = h2_Reflection.GetTypeByName("UnityEditor.ITreeViewDataSource")
                    );
            }
        }

        internal static Type _GameObjectTreeViewItemT;

        public static Type GameObjectTreeViewItemT
        {
            get
            {
                return _GameObjectTreeViewItemT ?? (
                    _GameObjectTreeViewItemT = h2_Reflection.GetTypeByName("UnityEditor.GameObjectTreeViewItem")
                    );
            }
        }

        internal static Type _GameObjectTreeViewDataSourceT;

        public static Type GameObjectTreeViewDataSourceT
        {
            get
            {
                return _GameObjectTreeViewDataSourceT ?? (
                    _GameObjectTreeViewDataSourceT = h2_Reflection.GetTypeByName("UnityEditor.GameObjectTreeViewDataSource")
                    );
            }
        }

        internal static EditorWindow FindEditor(string className)
        {
            var list = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var item in list)
            {
                //Debug.Log(item + ":" + item.GetType() + " ---> " + className + ":" + (item.GetType().FullName == className));
                if (item.GetType().FullName == className)
                {
                    return item;
                }
            }
            return null;
        }


        private static readonly string _hierarchyClassName = 
#if UNITY_4_5_OR_NEWER
            "UnityEditor.SceneHierarchyWindow";
#else
		"UnityEditor.HierarchyWindow";
#endif

        private static EditorWindow _hierarchy;

        public static EditorWindow HierarchyWindow
        {
            get
            {
                //BUGFIX : DO NOT USE _hierarchy ?? - Will not work when change layout !
                return _hierarchy != null ? _hierarchy : (_hierarchy = FindEditor(_hierarchyClassName));
            }
        }

        public static bool focusingHierarchy
        {
            get
            {
                var w = EditorWindow.focusedWindow;
                if (w == null) return false;
                return w.GetType().FullName == _hierarchyClassName;
            }
        }

        public static bool focusingInspector
        {
            get
            {
                var w = EditorWindow.focusedWindow;
                if (w == null) return false;
                return w.GetType().FullName == _inspectorClassName;
            }
        }


        private static readonly string _inspectorClassName = "UnityEditor.InspectorWindow";
        private static EditorWindow _inspector;

        public static EditorWindow InspectorWindow
        {
            get
            {
                //BUGFIX : DO NOT USE _inspector ?? - Will not work when change layout !
                return _inspector != null ? _inspector : (_inspector = FindEditor(_inspectorClassName));
            }
        }
    }
}