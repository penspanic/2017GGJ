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

using UnityEditor;
using UnityEngine;

namespace vietlabs.h2 {
internal class h2_Camera
{
	private static SceneView _sceneView;
	//camera being looked through & its saved information
	private static Camera lt_camera;
	public static bool lt_orthor;
	public static Vector3 lt_mPosition;
	public static Quaternion lt_mRotation;
	
	private static SceneView sceneView
	{
		get
		{
			if (_sceneView != null) return _sceneView;
                //if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof (SceneView)) return _sceneView = (SceneView) EditorWindow.focusedWindow;
			return _sceneView = SceneView.lastActiveSceneView ?? (SceneView) SceneView.sceneViews[0];
		}
	}
	
	private static Camera sceneCamera
	{
		get { return sceneView.camera; }
	}
	
	private static Vector3 m_Position
	{
		get { return GetAnimT<Vector3>("m_Position"); }
		set { SetAnimT("m_Position", FixNaN(value)); }
	}
	
	private static Quaternion m_Rotation
	{
		get { return GetAnimT<Quaternion>("m_Rotation"); }
		set { SetAnimT("m_Rotation", value); }
	}
	
	private static float cameraDistance
	{
		get { return (float) h2_Reflection.GetProperty(sceneView, "cameraDistance"); }
	}
	
	private static bool orthographic
	{
		get {
			var sv = sceneView;
			if (sv == null) return false;
			
			var svc = sv.camera;
			if (svc == null) return false;
			
			return svc.orthographic;
		}
		set
		{
                //sceneView.camera.orthographic = value;
#if UNITY_4_5_OR_NEWER
                SetAnimT("m_Ortho", value);
#else
			SetAnimT("m_Ortho", value ? 1f : 0f);
#endif
		}
	}
	
	public static void cmdLookThrough()
	{
		if (Selection.activeGameObject == null) return;
		var cam = Selection.activeGameObject.GetComponent<Camera>();
		if (cam == null) return;
		LookThrough(cam);
	}
	
	public static void cmdCaptureSV()
	{
		if (Selection.activeGameObject == null) return;
		var cam = Selection.activeGameObject.GetComponent<Camera>();
		if (cam == null) return;
		CaptureSceneView(cam);
	}
	
	static void Refresh()
	{
        //hacky way to force SceneView increase drawing frame
		var t = Selection.activeTransform
			?? (Camera.main != null ? Camera.main.transform : new GameObject("$t3mp$").transform);
		
		var op = t.position;
		t.position += new Vector3(1, 1, 1); //make some dirty
		t.position = op;
		
		if (t.name == "$t3mp$") Object.DestroyImmediate(t.gameObject, true);
	}
	
	public static void CopyTo(Camera c)
	{
		c.CopyFrom(sceneCamera);
	}
	
	public static void CopyFrom(Camera cam)
	{
		sceneCamera.CopyFrom(cam);
		sceneCamera.fieldOfView = cam.fieldOfView;
		
		orthographic = cam.orthographic;
		m_Rotation = cam.transform.rotation;
		m_Position = cam.transform.position - cam.transform.rotation*new Vector3(0, 0, -cameraDistance);
		Refresh();
	}
	
	
	private static T GetAnimT<T>(string name)
	{
		if (sceneView == null) return default(T);
		
		var animT = h2_Reflection.GetField(sceneView, name);
		
		return (T)
#if UNITY_4_5_OR_NEWER
                h2_Reflection.GetProperty(animT, "target");
#else
			h2_Reflection.GetField(animT, "m_Value");
#endif
	}
	
	private static void SetAnimT<T>(string name, T value)
	{
		if (sceneView == null) return;
		
		var animT = h2_Reflection.GetField(sceneView, name);
#if UNITY_4_5_OR_NEWER
            //animT.xSetProperty("target", value);
            h2_Reflection.SetProperty(animT, "target", value);
#else
		h2_Reflection.Invoke(animT, "BeginAnimating", null, null, value, h2_Reflection.GetField(animT, "m_Value"));
#endif
	}
	
	private static Vector3 FixNaN(Vector3 v)
	{
		if (float.IsNaN(v.x) || float.IsInfinity(v.x)) v.x = 0;
		if (float.IsNaN(v.y) || float.IsInfinity(v.y)) v.y = 0;
		if (float.IsNaN(v.z) || float.IsInfinity(v.z)) v.z = 0;
		return v;
	}
	
        /*private float m_Size {
            get { return GetAnimT<float>("m_Size"); }
            set { SetAnimT("m_Size", (float.IsInfinity(value) || (float.IsNaN(value)) || value == 0f) ? 100f : value); }
        }*/
	
	
	public static void CopyCamera(Camera dest, Camera src)
	{
		if (dest == null || src == null) return;
		
		dest.transform.position = src.transform.position;
		dest.transform.rotation = src.transform.rotation;
		dest.fieldOfView = src.fieldOfView;
		dest.orthographicSize = src.orthographicSize;
		
	        //dest.isOrthoGraphic = sceneCamera.isOrthoGraphic;
		dest.orthographic = src.orthographic;
	}
	
	
	public static void CaptureSceneView(Camera cam)
	{
		if (cam == null) return;
		
		lt_camera = null;
		Undo.RecordObject(cam, "Capture SceneView");
		Undo.RecordObject(cam.transform, "Capture SceneView");
		CopyCamera(cam, sceneCamera);
	}
	
	public static void LookThrough(Camera cam)
	{
		if (lt_camera == null)
		{
                //save current state of scene-camera
			lt_orthor = orthographic;
			lt_mPosition = m_Position;
			lt_mRotation = m_Rotation;
		}
		
		if (cam != lt_camera)
		{
			CopyFrom(cam);
			lt_camera = cam;
			
			if (Application.isPlaying)
			{
				EditorApplication.update -= UpdateLookThrough;
				EditorApplication.update += UpdateLookThrough;
			}
		}
		else if (lt_camera != null)
		{
            //cancel look through & restore old state of scene-camera
			orthographic = lt_orthor;
			m_Position = lt_mPosition;
			m_Rotation = lt_mRotation;
			lt_camera = null;
		}
	}
	
	static void UpdateLookThrough()
	{
		if (lt_camera == null || !EditorApplication.isPlaying)
		{
			lt_camera = null;
			EditorApplication.update -= UpdateLookThrough;
			return;
		}
		
		if (EditorApplication.isPaused) return;
		
		if (lt_camera.transform.position != sceneCamera.transform.position ||
			lt_camera.transform.rotation != sceneCamera.transform.rotation ||
			lt_camera.orthographic != sceneCamera.orthographic)
		{
			CopyFrom(lt_camera);
		}
	}
}
}