using UnityEngine;
using System.Collections;

public class CApplicationRestart : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void RestartAppForAOS()
	{
		AndroidJavaObject AOSUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
	
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaClass PendingIntentClass = new AndroidJavaClass("android.app.PendingIntent");
		AndroidJavaObject baseContext = AOSUnityActivity.Call<AndroidJavaObject>("getBaseContext");
		AndroidJavaObject intentObj
		 = baseContext.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage", baseContext.Call<string>("getPackageName"));
	 
		AndroidJavaObject context = AOSUnityActivity.Call<AndroidJavaObject>("getApplicationContext");
		AndroidJavaObject pendingIntentObj
		= PendingIntentClass.CallStatic<AndroidJavaObject>("getActivity", context, 123456, intentObj, PendingIntentClass.GetStatic<int>("FLAG_CANCEL_CURRENT"));
	
		AndroidJavaClass AlarmManagerClass = new AndroidJavaClass("android.app.AlarmManager");
		AndroidJavaClass JavaSystemClass = new AndroidJavaClass("java.lang.System");
	
		AndroidJavaObject mAlarmManager = AOSUnityActivity.Call<AndroidJavaObject>("getSystemService", "alarm");
		long restartMillis = JavaSystemClass.CallStatic<long>("currentTimeMillis") + 100;
		mAlarmManager.Call("set", AlarmManagerClass.GetStatic<int>("RTC"), restartMillis, pendingIntentObj);
	
		JavaSystemClass.CallStatic("exit", 0);

	}
}
