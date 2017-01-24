using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    [InitializeOnLoad]
    public class h2_Selection
    {
        private static bool inited;

        private static Action<GameObject[]> _callback;


        private static readonly float delayCheck = 0.01f; // 10 times per second
        static float lastUpdate;

        public static GameObject gameObject;
        public static GameObject[] gameObjects;
        private static Dictionary<int, GameObject> selectedGOMap;

        static h2_Selection()
        {
            Init();
        }

        public static string[] Selection_AssetGUIDs
        {
            get
            {
#if UNITY_5_0_OR_NEWER
		    	return Selection.assetGUIDs;
		    #else
                var mInfo = typeof(Selection).GetProperty("assetGUIDs",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (mInfo != null)
                {
                    return (string[]) mInfo.GetValue(null, null);
                }
                Debug.LogWarning("Unity changed ! Selection.assetGUIDs not found !");
                return new string[0];
#endif
            }
        }

        public static bool isMultiple
        {
            get { return gameObjects.Length > 1; }
        }

        public static void Register_OnSelectionChange(Action<GameObject[]> cb)
        {
            _callback -= cb;
            _callback += cb;

            if (!inited) Init();
        }

        static void Init()
        {
            if (inited) return;
            inited = true;
            selectedGOMap = new Dictionary<int, GameObject>();

            EditorApplication.update -= OnFrameUpdate;
            EditorApplication.update += OnFrameUpdate;
        }

        private static void OnFrameUpdate()
        {
            var realTime = Time.realtimeSinceStartup;
            if (realTime - lastUpdate < delayCheck) return;

            lastUpdate = realTime;
            CheckIfSelectionChanged();
        }

        public static bool Contains(int instID)
        {
            return selectedGOMap.ContainsKey(instID);
        }

        public static bool PartOfMuti(GameObject go, bool forceRefresh = true)
        {
            if (forceRefresh) CheckIfSelectionChanged();
            return gameObjects.Length > 1 && selectedGOMap.ContainsKey(go.GetInstanceID());
        }

        public static bool Contains(GameObject go)
        {
            if (go == null) return false;
            if (selectedGOMap == null) CheckIfSelectionChanged();
            return selectedGOMap.ContainsKey(go.GetInstanceID());
        }

        public static void CheckIfSelectionChanged()
        {
            if (gameObject != Selection.activeGameObject)
            {
                OnSelectionChange();
                return;
            }

            var newObjects = Selection.gameObjects;

            if (gameObjects == null || gameObjects.Length != newObjects.Length)
            {
                OnSelectionChange();
                return;
            }

            for (var i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] != newObjects[i])
                {
                    OnSelectionChange();
                    break;
                }
            }

            //Selection does not changed !
        }

        private static void OnSelectionChange()
        {
            gameObject = Selection.activeGameObject;
            gameObjects = Selection.gameObjects;
            selectedGOMap.Clear();

            for (var i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                selectedGOMap.Add(go.GetInstanceID(), go);
            }

            if (_callback != null) _callback(gameObjects);
            //Debug.Log("Selection changed :: " + selectedGOMap.Count);
        }
    }
}