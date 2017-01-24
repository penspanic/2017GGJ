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

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace vietlabs.h2
{
    public class h2_Asset
    {
        public static T FindAsset<T>(string assetName) where T : Object
        {
#if UNITY_5
            var possibleAssets = AssetDatabase.FindAssets(assetName);
#else
            string[] possibleAssets = new string[]
            {
                "Assets/Plugins/Editor/Hierarchy2/Graphics/" + assetName,
                "Assets/Plugins/Hierarchy2/Graphics/" + assetName,
                "Assets/Hierarchy2/Graphics/" + assetName
            };
#endif
            foreach (var p in possibleAssets)
            {
                var asset = LoadAssetAtPath<T>(p);
                if (asset != null) return asset;
            }
            return null;
        }

        public static string GetRelativePath(string from, string to)
        {
            if (from == to) return "[Self]";

            var fromArr = from.Split('/');
            var toArr = to.Split('/');

            var cnt = 0;
            var min = Mathf.Min(fromArr.Length, toArr.Length);

            //find the common path
            while (cnt < min && fromArr[cnt] == toArr[cnt])
            {
                cnt ++;
            }

            if (cnt == 0) return to;
            var result = string.Empty;
            for (var i = cnt; i < fromArr.Length; i++)
            {
                result += "../";
            }
            for (var i = cnt; i < toArr.Length; i++)
            {
                result += toArr[i] + "/";
            }
            return result;
        }

        public static T LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_5_1_OR_NEWER
			return AssetDatabase.LoadAssetAtPath<T>(path);
		#else
            return (T) AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }

        public static T LoadAssetWithGUID<T>(string guid) where T : Object
        {
            if (string.IsNullOrEmpty(guid)) return null;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_5_1_OR_NEWER
        	return AssetDatabase.LoadAssetAtPath<T>(path);
		#else
            return (T) AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }

        public static void UnloadUnusedAssets()
        {
#if UNITY_5_0_OR_NEWER
			EditorUtility.UnloadUnusedAssetsImmediate();
		#else
            EditorUtility.UnloadUnusedAssets();
#endif
            Resources.UnloadUnusedAssets();
        }

        public static T FindAssetOfType<T>(string assetName, string basePath, params string[] extensions)
            where T : Object
        {
            const int MAX_FAIL = 10; // call unload every 10 failed test-load

            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogWarning("Can not find asset without name ! " + typeof(T));
                return null;
            }

            assetName = assetName.Trim().ToLower();
            var checkpath = !string.IsNullOrEmpty(basePath);
            if (checkpath) basePath = basePath.ToLower();

            //generate extension map
            var dict = new Dictionary<string, int>();
            for (var i = 0; i < extensions.Length; i++)
            {
                dict.Add(extensions[i].ToLower(), i);
            }

            var checkExt = dict.Count > 0;

            var gc = 0;
            var candidateList = new List<string>();

            var paths = AssetDatabase.GetAllAssetPaths();

            //try to match name / extensions
            foreach (string t in paths)
            {
                var p = t.Replace("\\", "/").ToLower();

                if (checkExt)
                {
                    var ext = Path.GetExtension(p);
                    if (!dict.ContainsKey(ext)) continue; // unmatched extension
                }

                if (checkpath)
                {
                    if (!p.Contains(basePath)) continue; // unmatched path
                }

                var name = Path.GetFileNameWithoutExtension(p);
                if (name == assetName)
                {
                    //exact match : load now !       
                    var asset = LoadAssetAtPath<T>(t);
                    if (asset != null)
                    {
                        UnloadUnusedAssets();
                        return asset;
                    }

                    Debug.Log("Exact match found but can not load <" + t + ">\n" + assetName + "\n" + basePath);
                    gc++;

                    if (gc > MAX_FAIL)
                    {
                        // Unload test-load-failed assets
                        gc = 0;
                        UnloadUnusedAssets();
                    }
                }

                if (!name.Contains(assetName)) continue; // unmatched name
                candidateList.Add(p);
            }

            //no exact match found, search for first valid candidate
            foreach (string t in candidateList)
            {
                var asset = LoadAssetAtPath<T>(t);
                if (asset != null)
                {
                    UnloadUnusedAssets();
                    return asset;
                }

                gc++;

                if (gc > MAX_FAIL)
                {
                    // Unload test-load-failed assets
                    gc = 0;
                    UnloadUnusedAssets();
                }
            }

            return null;
        }

        public static List<T> FindAssetOfTypeAll<T>(string assetName = null, params string[] extensions)
            where T : Object
        {
            var paths = AssetDatabase.GetAllAssetPaths();

            //generate extension map
            var dict = new Dictionary<string, int>();
            for (var i = 0; i < extensions.Length; i++)
            {
                dict.Add(extensions[i].ToLower(), i);
            }

            var result = new List<T>();
            var checkName = false;
            var checkExt = dict.Count > 0;

            if (!string.IsNullOrEmpty(assetName))
            {
                checkName = true;
                assetName = assetName.ToLower();
            }

            if (!checkName && !checkExt)
            {
                Debug.LogWarning(
                    "FindAsset without name and extension is very heavy, avoid it at all cost ! This will scan the whole project and load each asset into memory to see if it's the correct type !");
                return result;
            }

            var gc = 0;
            foreach (string t in paths)
            {
                var p = t.ToLower();

                if (checkExt && !dict.ContainsKey(Path.GetExtension(p))) continue;
                if (checkName && !p.Contains(assetName)) continue;

                //try to load path 
                var asset = LoadAssetAtPath<T>(t);
                if (asset != null)
                {
                    result.Add(asset);
                    continue;
                }
                gc ++;

                if (gc > 10) // Unload test-load assets
                {
                    gc = 0;
                    UnloadUnusedAssets();
                }
            }

            return result;
        }
    }
}