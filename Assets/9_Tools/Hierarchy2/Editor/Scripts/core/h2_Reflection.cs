using System;
using System.Reflection;
using UnityEngine;

namespace vietlabs.h2
{
    public class h2_Reflection
    {
        private const BindingFlags AllFlags = BindingFlags.Default //| BindingFlags.ExactBinding
                                              | BindingFlags.FlattenHierarchy //| BindingFlags.DeclaredOnly
            //| BindingFlags.CreateInstance
            //| BindingFlags.GetField
            //| BindingFlags.GetProperty
            //| BindingFlags.IgnoreCase
            //| BindingFlags.IgnoreReturn
            //| BindingFlags.SuppressChangeType
            //| BindingFlags.InvokeMethod
                                              | BindingFlags.NonPublic | BindingFlags.Public
            //| BindingFlags.OptionalParamBinding
            //| BindingFlags.PutDispProperty
            //| BindingFlags.PutRefDispProperty
            //| BindingFlags.SetField
            //| BindingFlags.SetProperty
                                              | BindingFlags.Instance | BindingFlags.Static;

        public static bool HasMethod(object obj, string methodName, Type type = null, BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(methodName)) return false;
            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            return type.GetMethod(methodName, flags ?? AllFlags) != null;
        }

        public static object Invoke(object obj, string methodName, Type type = null, BindingFlags? flags = null,
            params object[] parameters)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            var f = type.GetMethod(methodName, flags ?? AllFlags);

            if (f != null) return f.Invoke(obj, parameters);
            Debug.LogWarning(string.Format("Invoke Error : <{0}> is not a method of type <{1}>", methodName, type));
            return null;
        }

        public static object Invoke(object obj, string methodName, Type type, Type[] typeParams,
            BindingFlags? flags = null, params object[] parameters)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            var f = type.GetMethod(methodName, flags ?? AllFlags, null, CallingConventions.Standard, typeParams, null);

            if (f != null) return f.Invoke(obj, parameters);
            Debug.LogWarning(string.Format("Invoke Error : <{0}> is not a method of type <{1}>", methodName, type));
            return null;
	        /*
	        ArgumentException: failed to convert parameters
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:192)
System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MethodBase.cs:115)
vietlabs.h2.h2_Reflection.Invoke (System.Object obj, System.String methodName, System.Type type, Nullable`1 flags, System.Object[] parameters) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Reflection.cs:41)
vietlabs.h2.h2_Camera.SetAnimT[Single] (System.String name, Single value) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:368)
vietlabs.h2.h2_Camera.set_orthographic (Boolean value) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:294)
vietlabs.h2.h2_Camera.CopyFrom (UnityEngine.Camera cam) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:338)
vietlabs.h2.h2_Camera.LookThrough (UnityEngine.Camera cam) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:416)
vietlabs.h2.h2_Common.RunCommand (System.String cmd) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:56)
vietlabs.h2.h2_MatchData.Trigger (System.Collections.Generic.List`1 matchList) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Shortcut.cs:201)
vietlabs.h2.h2_MatchData.Check (UnityEngine.Event e, System.Collections.Generic.Dictionary`2 hndMap) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Shortcut.cs:169)
vietlabs.h2.h2_Shortcut.Check () (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Shortcut.cs:84)
vietlabs.h2.Hierarchy2.HierarchyItemCB (Int32 instID, Rect r) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/Hierarchy2.cs:222)
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:222)
Rethrow as TargetInvocationException: Exception has been thrown by the target of an invocation.
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:232)
System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MethodBase.cs:115)
System.Delegate.DynamicInvokeImpl (System.Object[] args) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System/Delegate.cs:443)
System.MulticastDelegate.DynamicInvokeImpl (System.Object[] args) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System/MulticastDelegate.cs:71)
System.Delegate.DynamicInvoke (System.Object[] args) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System/Delegate.cs:415)
FavoritesTab+TreeViewTracker.HierarchyItemOnGuiCallback (Int32 item, Rect selectionRect) (at Assets/FlipbookGames/FavoritesTab/Editor/Scripts/FavoritesTab.cs:1754)
UnityEditor.SceneHierarchyWindow.OnGUIAssetCallback (Int32 instanceID, Rect rect) (at C:/buildslave/unity/build/Editor/Mono/SceneHierarchyWindow.cs:230)
UnityEditor.TreeView.OnGUI (Rect rect, Int32 keyboardControlID) (at C:/buildslave/unity/build/Editor/Mono/GUI/TreeView/TreeView.cs:404)
UnityEditor.SceneHierarchyWindow.DoTreeView (Single searchPathHeight) (at C:/buildslave/unity/build/Editor/Mono/SceneHierarchyWindow.cs:334)
UnityEditor.SceneHierarchyWindow.OnGUI () (at C:/buildslave/unity/build/Editor/Mono/SceneHierarchyWindow.cs:178)
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:222)
	        
	        */
        }
	    
	    public static Type GetTypeByName(string typeName)
	    {
		    var asmList = AppDomain.CurrentDomain.GetAssemblies();
		    for (var i = 0; i < asmList.Length; i++)
		    {
			    var result = asmList[i].GetType(typeName);
			    if (result != null) return result;
		    }
		    return null;
	    }

        public static T ChangeType<T>(object obj)
        {
            return (T) Convert.ChangeType(obj, typeof(T));
        }

        public static bool HasField(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return false;
            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            return type.GetField(name, flags ?? AllFlags) != null;
        }

        public static object GetField(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return false;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            var field = type.GetField(name, flags ?? AllFlags);
            if (field == null)
            {
                Debug.LogWarning(
                    string.Format("GetField Error : <{0}> does not contains a field with name <{1}>", type, name));
                return null;
            }

            return field.GetValue(obj);
        }

        public static void SetField(object obj, string name, object value, Type type = null, BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            var field = type.GetField(name, flags ?? AllFlags);

            if (field == null)
            {
                Debug.LogWarning(
                    string.Format("SetField Error : <{0}> does not contains a field with name <{1}>", type, name));
                return;
            }

            field.SetValue(obj, value);
        }

        public static bool HasProperty(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return false;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            return type.GetProperty(name, flags ?? AllFlags) != null;
        }

        public static void SetProperty(object obj, string name, object value, Type type = null,
            BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            var property = type.GetProperty(name, flags ?? AllFlags);

            if (property == null)
            {
                Debug.LogWarning(
                    string.Format("SetProperty Error : <{0}> does not contains a property with name <{1}>", obj, name));
                return;
            }

            property.SetValue(obj, value, null);
        }

        public static object GetProperty(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return null;

            if (type == null) type = obj is Type ? (Type) obj : obj.GetType();
            var property = type.GetProperty(name, flags ?? AllFlags);
            if (property != null) return property.GetValue(obj, null);

            Debug.LogWarning(
                string.Format("GetProperty Error : <{0}> does not contains a property with name <{1}>", type, name));
            return null;
        }
    }
}